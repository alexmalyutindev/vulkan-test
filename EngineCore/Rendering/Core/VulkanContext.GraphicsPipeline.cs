using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace EngineCore.Rendering.Core;

public sealed unsafe partial class VulkanContext
{
    public class GraphicsPipeline
    {
        private readonly VulkanContext _context;
        private readonly Vk _vk;
        private readonly VulkanDevice _device;

        private Pipeline _graphicsPipeline;

        // TODO: ???
        private DescriptorSetLayout _descriptorSetLayout;
        private PipelineLayout _pipelineLayout;

        public GraphicsPipeline(VulkanContext context)
        {
            _context = context;

            _vk = context._vk;
            _device = context._device;
        }

        public void CreateDescriptorSetLayout()
        {
            DescriptorSetLayoutBinding uboLayoutBinding = new()
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            DescriptorSetLayoutBinding samplerLayoutBinding = new()
            {
                Binding = 1,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.FragmentBit,
            };

            var bindings = new DescriptorSetLayoutBinding[] { uboLayoutBinding, samplerLayoutBinding };

            fixed (DescriptorSetLayoutBinding* bindingsPtr = bindings)
            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &_descriptorSetLayout)
            {
                DescriptorSetLayoutCreateInfo layoutInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint) bindings.Length,
                    PBindings = bindingsPtr,
                };

                if (_vk!.CreateDescriptorSetLayout(_device.LogicalDevice, layoutInfo, null, descriptorSetLayoutPtr) !=
                    Result.Success)
                {
                    throw new Exception("Failed to create descriptor set layout!");
                }
            }
        }

        public void CreateGraphicsPipeline(RenderPass renderPass, string vertex, string fragment, Extent2D swapChainExtent)
        {
            var vertShaderCode = File.ReadAllBytes(vertex);
            var fragShaderCode = File.ReadAllBytes(fragment);

            var vertShaderModule = _context.CreateShaderModule(vertShaderCode);
            var fragShaderModule = _context.CreateShaderModule(fragShaderCode);

            PipelineShaderStageCreateInfo vertShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertShaderModule,
                PName = (byte*) SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo fragShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragShaderModule,
                PName = (byte*) SilkMarshal.StringToPtr("main")
            };

            var shaderStages = stackalloc[]
            {
                vertShaderStageInfo,
                fragShaderStageInfo
            };

            var bindingDescription = Attributes.GetBindingDescription();
            var attributeDescriptions = Attributes.GetAttributeDescriptions();

            fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &_descriptorSetLayout)
            {
                PipelineVertexInputStateCreateInfo vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    VertexAttributeDescriptionCount = (uint) attributeDescriptions.Length,
                    PVertexBindingDescriptions = &bindingDescription,
                    PVertexAttributeDescriptions = attributeDescriptionsPtr,
                };

                PipelineInputAssemblyStateCreateInfo inputAssembly = new()
                {
                    SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                    Topology = PrimitiveTopology.TriangleList,
                    PrimitiveRestartEnable = false,
                };

                Viewport viewport = new()
                {
                    X = 0,
                    Y = 0,
                    Width = swapChainExtent.Width,
                    Height = swapChainExtent.Height,
                    MinDepth = 0,
                    MaxDepth = 1,
                };

                Rect2D scissor = new()
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapChainExtent,
                };

                PipelineViewportStateCreateInfo viewportState = new()
                {
                    SType = StructureType.PipelineViewportStateCreateInfo,
                    ViewportCount = 1,
                    PViewports = &viewport,
                    ScissorCount = 1,
                    PScissors = &scissor,
                };

                PipelineRasterizationStateCreateInfo rasterizer = new()
                {
                    SType = StructureType.PipelineRasterizationStateCreateInfo,
                    DepthClampEnable = false,
                    RasterizerDiscardEnable = false,
                    PolygonMode = PolygonMode.Fill,
                    LineWidth = 1,
                    CullMode = CullModeFlags.BackBit,
                    FrontFace = FrontFace.CounterClockwise,
                    DepthBiasEnable = false,
                };

                PipelineMultisampleStateCreateInfo multisampling = new()
                {
                    SType = StructureType.PipelineMultisampleStateCreateInfo,
                    SampleShadingEnable = false,
                    RasterizationSamples = SampleCountFlags.Count1Bit,
                };

                PipelineDepthStencilStateCreateInfo depthStencil = new()
                {
                    SType = StructureType.PipelineDepthStencilStateCreateInfo,
                    DepthTestEnable = true,
                    DepthWriteEnable = true,
                    DepthCompareOp = CompareOp.Less,
                    DepthBoundsTestEnable = false,
                    StencilTestEnable = false,
                };

                PipelineColorBlendAttachmentState colorBlendAttachment = new()
                {
                    ColorWriteMask =
                        ColorComponentFlags.RBit | ColorComponentFlags.GBit |
                        ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                    BlendEnable = false,
                };

                PipelineColorBlendStateCreateInfo colorBlending = new()
                {
                    SType = StructureType.PipelineColorBlendStateCreateInfo,
                    LogicOpEnable = false,
                    LogicOp = LogicOp.Copy,
                    AttachmentCount = 1,
                    PAttachments = &colorBlendAttachment,
                };

                colorBlending.BlendConstants[0] = 0;
                colorBlending.BlendConstants[1] = 0;
                colorBlending.BlendConstants[2] = 0;
                colorBlending.BlendConstants[3] = 0;

                PipelineLayoutCreateInfo pipelineLayoutInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    PushConstantRangeCount = 0,
                    SetLayoutCount = 1,
                    PSetLayouts = descriptorSetLayoutPtr
                };

                if (_context.CreatePipelineLayout(pipelineLayoutInfo, out _pipelineLayout) != Result.Success)
                {
                    throw new Exception("failed to create pipeline layout!");
                }

                var pipelineInfo = new GraphicsPipelineCreateInfo()
                {
                    SType = StructureType.GraphicsPipelineCreateInfo,
                    StageCount = 2,
                    PStages = shaderStages,
                    PVertexInputState = &vertexInputInfo,
                    PInputAssemblyState = &inputAssembly,
                    PViewportState = &viewportState,
                    PRasterizationState = &rasterizer,
                    PMultisampleState = &multisampling,
                    PDepthStencilState = &depthStencil,
                    PColorBlendState = &colorBlending,
                    Layout = _pipelineLayout,
                    RenderPass = renderPass.Handle,
                    Subpass = 0,
                    BasePipelineHandle = default
                };

                if (_context.CreateGraphicsPipeline(pipelineInfo, out _graphicsPipeline) != Result.Success)
                {
                    throw new Exception("failed to create graphics pipeline!");
                }
            }
            
            _vk!.DestroyShaderModule(_device.LogicalDevice, fragShaderModule, null);
            _vk!.DestroyShaderModule(_device.LogicalDevice, vertShaderModule, null);

            SilkMarshal.Free((nint) vertShaderStageInfo.PName);
            SilkMarshal.Free((nint) fragShaderStageInfo.PName);
        }

        public void Destroy()
        {
            _vk!.DestroyPipelineLayout(_device.LogicalDevice, _pipelineLayout, null);
            _vk!.DestroyDescriptorSetLayout(_device.LogicalDevice, _descriptorSetLayout, null);
            _vk.DestroyPipeline(_device.LogicalDevice, _graphicsPipeline, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result CreatePipelineLayout(PipelineLayoutCreateInfo pipelineLayoutInfo, out PipelineLayout pipelineLayout)
    {
        return _vk!.CreatePipelineLayout(_device.LogicalDevice, pipelineLayoutInfo, null, out pipelineLayout);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Result CreateGraphicsPipeline(GraphicsPipelineCreateInfo pipelineInfo, out Pipeline graphicsPipeline)
    {
        return _vk!.CreateGraphicsPipelines(
            _device.LogicalDevice,
            default,
            1,
            pipelineInfo,
            null,
            out graphicsPipeline
        );
    }

    private ShaderModule CreateShaderModule(byte[] code)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint) code.Length,
        };

        ShaderModule shaderModule;

        fixed (byte* codePtr = code)
        {
            createInfo.PCode = (uint*) codePtr;

            if (_vk!.CreateShaderModule(_device.LogicalDevice, createInfo, null, out shaderModule) != Result.Success)
            {
                throw new Exception();
            }
        }

        return shaderModule;
    }
}