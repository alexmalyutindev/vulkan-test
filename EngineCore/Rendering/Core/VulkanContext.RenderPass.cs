using Silk.NET.Vulkan;

namespace EngineCore.Rendering.Core;

public partial class VulkanContext
{
    public class RenderPass
    {
        public Silk.NET.Vulkan.RenderPass Handle => _renderPass;

        private readonly VulkanContext _context;
        private Silk.NET.Vulkan.RenderPass _renderPass;

        public RenderPass(VulkanContext context)
        {
            _context = context;
            // TODO: Make it configurable
        }

        public unsafe void CreateRenderPass(Format colorFormat)
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = colorFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            AttachmentDescription depthAttachment = new()
            {
                Format = _context.FindDepthFormat(),
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            AttachmentReference colorAttachmentRef = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            AttachmentReference depthAttachmentRef = new()
            {
                Attachment = 1,
                Layout = ImageLayout.DepthStencilAttachmentOptimal,
            };

            SubpassDescription subPass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentRef,
                PDepthStencilAttachment = &depthAttachmentRef,
            };

            SubpassDependency dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit |
                    PipelineStageFlags.EarlyFragmentTestsBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit |
                    PipelineStageFlags.EarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
            };

            var attachments = new[] { colorAttachment, depthAttachment };

            fixed (AttachmentDescription* attachmentsPtr = attachments)
            {
                RenderPassCreateInfo renderPassInfo = new()
                {
                    SType = StructureType.RenderPassCreateInfo,
                    AttachmentCount = (uint) attachments.Length,
                    PAttachments = attachmentsPtr,
                    SubpassCount = 1,
                    PSubpasses = &subPass,
                    DependencyCount = 1,
                    PDependencies = &dependency,
                };

                if (_context._vk.CreateRenderPass(
                        _context._device.LogicalDevice,
                        renderPassInfo,
                        null,
                        out _renderPass
                    ) !=
                    Result.Success)
                {
                    throw new Exception("Failed to create render pass!");
                }
            }
        }

        public void BeginPass() { }

        public void EndPass() { }

        public void Destroy()
        {
            // TODO: Destroy render pass.
        }
    }
}