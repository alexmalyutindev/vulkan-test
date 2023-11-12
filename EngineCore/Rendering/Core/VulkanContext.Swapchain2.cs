using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;

namespace EngineCore.Rendering.Core;

public unsafe partial class VulkanContext
{
    public class Swapchain2
    {
        public Extent2D SwapChainExtent => _swapChainExtent;
        public Format SwapChainImageFormat => _swapChainImageFormat;

        private readonly VulkanContext _context;
        private readonly VulkanDevice _device;

        private KhrSwapchain? _khrSwapChain;
        private SwapchainKHR _swapChain;

        private Extent2D _swapChainExtent;
        private Format _swapChainImageFormat;

        private Image[]? _swapChainImages;
        private ImageView[]? _swapChainImageViews;

        public Swapchain2(VulkanContext context)
        {
            _context = context;
            _device = _context._device;
        }

        public void Initialize()
        {
            CreateSwapChain();
            CreateImageViews();
        }

        public void Recreate()
        {
            Destroy();
            Initialize();
        }

        // TODO: AcquireNextImageIndex
        public bool AcquireNextImageIndex(VulkanContext context, out uint index)
        {
            index = 0;
            return false;
        }

        // TODO: Present
        public void Present(VulkanContext context) { }

        public void Destroy()
        {
            var vk = _context._vk;

            foreach (var imageView in _swapChainImageViews!)
            {
                vk.DestroyImageView(_device.LogicalDevice, imageView, null);
            }

            _khrSwapChain!.DestroySwapchain(_device.LogicalDevice, _swapChain, null);
        }

        private void CreateSwapChain()
        {
            var swapChainSupport = _context.QuerySwapChainSupport(_device.PhysicalDevice);

            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
            var extent = _context.ChooseSwapExtent(swapChainSupport.Capabilities);

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 &&
                imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            SwapchainCreateInfoKHR createInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,
                Surface = _context._surface,

                MinImageCount = imageCount,
                ImageFormat = surfaceFormat.Format,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageExtent = extent,
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            };

            var indices = _context.FindQueueFamilies(_device.PhysicalDevice);
            var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };

            if (indices.GraphicsFamily != indices.PresentFamily)
            {
                createInfo = createInfo with
                {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            }
            else
            {
                createInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            createInfo = createInfo with
            {
                PreTransform = swapChainSupport.Capabilities.CurrentTransform,
                CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
                PresentMode = presentMode,
                Clipped = true,
            };

            if (_khrSwapChain is null)
            {
                if (!_device.TryGetDeviceExtension(out _khrSwapChain))
                {
                    throw new NotSupportedException("VK_KHR_swapchain extension not found.");
                }
            }

            if (_khrSwapChain!.CreateSwapchain(
                    _device.LogicalDevice,
                    createInfo,
                    null,
                    out _swapChain
                ) != Result.Success)
            {
                throw new Exception("failed to create swap chain!");
            }

            _khrSwapChain.GetSwapchainImages(_device.LogicalDevice, _swapChain, ref imageCount, null);
            _swapChainImages = new Image[imageCount];
            fixed (Image* swapChainImagesPtr = _swapChainImages)
            {
                _khrSwapChain.GetSwapchainImages(_device.LogicalDevice, _swapChain, ref imageCount, swapChainImagesPtr);
            }

            _swapChainImageFormat = surfaceFormat.Format;
            _swapChainExtent = extent;
        }

        public void CreateImageViews()
        {
            _swapChainImageViews = new ImageView[_swapChainImages!.Length];

            for (int i = 0; i < _swapChainImages.Length; i++)
            {
                _swapChainImageViews[i] = _context.CreateImageView(
                    _swapChainImages[i],
                    _swapChainImageFormat,
                    ImageAspectFlags.ColorBit
                );
            }
        }
    }
}
