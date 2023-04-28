using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;

namespace RenderCore.RenderModule;

public unsafe partial class VulkanContext
{
    public class Swapchain
    {
        public KhrSwapchain? _khrSwapChain;
        public SwapchainKHR _swapChain;
        public Image[]? _swapChainImages;
        public Format _swapChainImageFormat;
        public Extent2D _swapChainExtent;
        public ImageView[]? _swapChainImageViews;
        public Framebuffer[]? _swapChainFramebuffers;

        private readonly VulkanContext _context;
        private readonly VulkanDevice _device;

        public Swapchain(VulkanContext context, VulkanDevice device)
        {
            _context = context;
            _device = device;
            CreateSwapChain();
            CreateImageViews();
        }

        public void Recreate()
        {
            // TODO:
        }

        public bool AcquireNextImageIndex(VulkanContext context, out uint index)
        {
            index = 0;
            return false;
        }

        public void Present(VulkanContext context)
        {
            // TODO:
        }

        public void Destroy(Vk vk)
        {
            // TODO:
            // vk.DestroyImageView(_device, _depthImageView, null);
            // vk.DestroyImage(_device, _depthImage, null);
            // vk.FreeMemory(_device, _depthImageMemory, null);

            foreach (var framebuffer in _swapChainFramebuffers!)
            {
                vk.DestroyFramebuffer(_device.LogicalDevice, framebuffer, null);
            }

            // fixed (CommandBuffer* commandBuffersPtr = _commandBuffers)
            // {
            //     vk.FreeCommandBuffers(_device, _commandPool, (uint) _commandBuffers!.Length, commandBuffersPtr);
            // }

            // vk.DestroyPipeline(_device, _graphicsPipeline, null);
            // vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
            // vk.DestroyRenderPass(_device, _renderPass, null);

            foreach (var imageView in _swapChainImageViews!)
            {
                vk.DestroyImageView(_device.LogicalDevice, imageView, null);
            }

            _khrSwapChain!.DestroySwapchain(_device.LogicalDevice, _swapChain, null);

            // for (int i = 0; i < _swapChainImages!.Length; i++)
            // {
            //     vk.DestroyBuffer(_device, _uniformBuffers![i], null);
            //     vk.FreeMemory(_device, _uniformBuffersMemory![i], null);
            // }
            //
            // vk.DestroyDescriptorPool(_device, _descriptorPool, null);
        }

        private void CreateSwapChain()
        {
            var swapChainSupport = _context.QuerySwapChainSupport(_device.PhysicalDevice);

            var surfaceFormat = _context.ChooseSwapSurfaceFormat(swapChainSupport.Formats);
            var presentMode = _context.ChoosePresentMode(swapChainSupport.PresentModes);
            var extent = _context.ChooseSwapExtent(swapChainSupport.Capabilities);

            var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
            if (swapChainSupport.Capabilities.MaxImageCount > 0 &&
                imageCount > swapChainSupport.Capabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.Capabilities.MaxImageCount;
            }

            SwapchainCreateInfoKHR creatInfo = new()
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
                creatInfo = creatInfo with
                {
                    ImageSharingMode = SharingMode.Concurrent,
                    QueueFamilyIndexCount = 2,
                    PQueueFamilyIndices = queueFamilyIndices,
                };
            }
            else
            {
                creatInfo.ImageSharingMode = SharingMode.Exclusive;
            }

            creatInfo = creatInfo with
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
                    creatInfo,
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

        private void CreateImageViews()
        {
            _swapChainImageViews = new ImageView[_swapChainImages!.Length];

            for (int i = 0; i < _swapChainImages.Length; i++)
            {
                _swapChainImageViews[i] = CreateImageView(
                    _swapChainImages[i],
                    _swapChainImageFormat,
                    ImageAspectFlags.ColorBit
                );
            }
        }

        private ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.Type2D,
                Format = format,
                //Components =
                //    {
                //        R = ComponentSwizzle.Identity,
                //        G = ComponentSwizzle.Identity,
                //        B = ComponentSwizzle.Identity,
                //        A = ComponentSwizzle.Identity,
                //    },
                SubresourceRange =
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            if (_context._vk!.CreateImageView(
                    _device.LogicalDevice,
                    createInfo,
                    null,
                    out var imageView
                ) !=
                Result.Success)
            {
                throw new Exception("failed to create image views!");
            }

            return imageView;
        }
    }

    private SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
    {
        var details = new SwapChainSupportDetails();

        _khrSurface!.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, _surface, out details.Capabilities);

        uint formatCount = 0;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _surface, ref formatCount, null);

        if (formatCount != 0)
        {
            details.Formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
            {
                _khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, _surface, ref formatCount, formatsPtr);
            }
        }
        else
        {
            details.Formats = Array.Empty<SurfaceFormatKHR>();
        }

        uint presentModeCount = 0;
        _khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, _surface, ref presentModeCount, null);

        if (presentModeCount != 0)
        {
            details.PresentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* formatsPtr = details.PresentModes)
            {
                _khrSurface.GetPhysicalDeviceSurfacePresentModes(
                    physicalDevice,
                    _surface,
                    ref presentModeCount,
                    formatsPtr
                );
            }
        }
        else
        {
            details.PresentModes = Array.Empty<PresentModeKHR>();
        }

        return details;
    }

    private struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }

    private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
    {
        foreach (var availableFormat in availableFormats)
        {
            if (availableFormat.Format == Format.B8G8R8A8Srgb &&
                availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return availableFormat;
            }
        }

        return availableFormats[0];
    }

    private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
    {
        foreach (var availablePresentMode in availablePresentModes)
        {
            if (availablePresentMode == PresentModeKHR.MailboxKhr)
            {
                return availablePresentMode;
            }
        }

        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }
        else
        {
            var framebufferSize = _window!.FramebufferSize;

            Extent2D actualExtent = new()
            {
                Width = (uint) framebufferSize.X,
                Height = (uint) framebufferSize.Y
            };

            actualExtent.Width = Math.Clamp(
                actualExtent.Width,
                capabilities.MinImageExtent.Width,
                capabilities.MaxImageExtent.Width
            );
            actualExtent.Height = Math.Clamp(
                actualExtent.Height,
                capabilities.MinImageExtent.Height,
                capabilities.MaxImageExtent.Height
            );

            return actualExtent;
        }
    }
}