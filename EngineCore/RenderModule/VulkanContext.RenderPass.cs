using Silk.NET.Vulkan;

namespace RenderCore.RenderModule;

public partial class VulkanContext
{
    // This is an entry point to render loop
    // It contain Swapchain that would be blited on the screen
    public class RenderPass
    {
        private readonly VulkanContext _context;
        public Silk.NET.Vulkan.RenderPass _renderPass;
        private Swapchain _swapchain;

        public RenderPass(VulkanContext context)
        {
            _context = context;
            // TODO: Make it configurable
            _swapchain = new Swapchain(_context, this);
        }

        public void Initialize()
        {
            // TODO: Unravel this dependency, maybe move on top level.
            _swapchain.CreateSwapChain();
            _swapchain.CreateImageViews();
            CreateRenderPass();
            
            // TODO: CreateDescriptorSetLayout();
            // TODO: CreateGraphicsPipeline();
            // TODO: CreateCommandPool();
            _swapchain.CreateDepthResources();

            _swapchain.CreateFramebuffers();
        }

        private unsafe void CreateRenderPass()
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = _swapchain._swapChainImageFormat,
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

                if (_context._vk!.CreateRenderPass(
                        _context._device.LogicalDevice,
                        renderPassInfo,
                        null,
                        out _renderPass
                    ) !=
                    Result.Success)
                {
                    throw new Exception("failed to create render pass!");
                }
            }
        }
        
        

        public void BeginPass() { }

        public void EndPass() { }

        public void Destroy()
        {
            _swapchain.Destroy(_context._vk);
        }
    }
}