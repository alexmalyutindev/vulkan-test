using Silk.NET.Vulkan;

namespace RenderCore.RenderModule;

public partial class VulkanContext
{
    // This is an entry point to render loop
    // It contain Swapchain that would be blited on the screen
    public class RenderPass
    {
        private readonly VulkanContext _context;
        private Silk.NET.Vulkan.RenderPass _renderPass;
        private Swapchain _swapchain;

        public RenderPass(VulkanContext context)
        {
            _context = context;
            _swapchain = new Swapchain(context, _renderPass);
        }

        public void Initialize()
        {
            _swapchain.Recreate();
        }

        public void BeginPass()
        {
            
        }
        
        public void EndPass()
        {
            
        }

        public void Destroy()
        {
            _swapchain.Destroy(_context._vk);
        }
    }
}