using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Image = Silk.NET.Vulkan.Image;

namespace EngineCore.Rendering.Core;

public unsafe partial class VulkanContext
{
    public class Swapchain
    {
        public KhrSwapchain Handle => _khrSwapChain!;
        public Format Format => _swapChainImageFormat;
        public Extent2D Extent => _swapChainExtent;

        private KhrSwapchain? _khrSwapChain;
        private SwapchainKHR _swapChain;
        private Format _swapChainImageFormat;
        private Extent2D _swapChainExtent;
        
        private Image[]? _swapChainImages;
        private ImageView[]? _swapChainImageViews;
        private Framebuffer[]? _swapChainFramebuffers;
        private CommandBuffer[]? _commandBuffers;

        // ???
        private Image _depthImage;
        private ImageView _depthImageView;
        private DeviceMemory _depthImageMemory;

        private readonly Vk _vk;
        private readonly VulkanContext _context;
        private readonly VulkanDevice _device;

        public Swapchain(VulkanContext context)
        {
            _context = context;
            _vk = context._vk;
            _device = context._device;
        }

        public void Recreate()
        {
            // TODO:
            // _swapChainFramebuffers = Array.Empty<Framebuffer>();
        }

        public bool AcquireNextImageIndex(VulkanContext context, out uint index)
        {
            index = 0;
            return false;
        }

        public void Present()
        {
            // TODO:
        }

        public void Destroy()
        {
            // TODO: Depth
            // vk.DestroyImageView(_device, _depthImageView, null);
            // vk.DestroyImage(_device, _depthImage, null);
            // vk.FreeMemory(_device, _depthImageMemory, null);

            foreach (var framebuffer in _swapChainFramebuffers!)
            {
                _vk.DestroyFramebuffer(_device.LogicalDevice, framebuffer, null);
            }

            // TODO: Command Buffers!
            // fixed (CommandBuffer* commandBuffersPtr = _commandBuffers)
            // {
            //     vk.FreeCommandBuffers(_device, _commandPool, (uint) _commandBuffers!.Length, commandBuffersPtr);
            // }

            // vk.DestroyPipeline(_device, _graphicsPipeline, null);
            // vk.DestroyPipelineLayout(_device, _pipelineLayout, null);
            // vk.DestroyRenderPass(_device, _renderPass, null);

            foreach (var imageView in _swapChainImageViews!)
            {
                _vk.DestroyImageView(_device.LogicalDevice, imageView, null);
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

        public void CreateSwapChain()
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
                _swapChainImageViews[i] = CreateImageView(
                    _swapChainImages[i],
                    _swapChainImageFormat,
                    ImageAspectFlags.ColorBit
                );
            }
        }

        public void CreateDepthResources()
        {
            Format depthFormat = _context.FindDepthFormat();

            _context.CreateImage(
                _swapChainExtent.Width,
                _swapChainExtent.Height,
                depthFormat,
                ImageTiling.Optimal,
                ImageUsageFlags.DepthStencilAttachmentBit,
                MemoryPropertyFlags.DeviceLocalBit,
                ref _depthImage,
                ref _depthImageMemory
            );
            _depthImageView = CreateImageView(_depthImage, depthFormat, ImageAspectFlags.DepthBit);
        }

        // TODO: Move to RenderPass, looks like depth and image view is part of pass, not a swapchain
        public void CreateFramebuffers(RenderPass renderPass)
        {
            _swapChainFramebuffers = new Framebuffer[_swapChainImageViews!.Length];

            // TODO: RenderPass and RenderSubPass
            for (int i = 0; i < _swapChainImageViews.Length; i++)
            {
                // TODO: Configure targets
                ImageView[] attachments = { _swapChainImageViews[i], _depthImageView };

                fixed (ImageView* attachmentsPtr = attachments)
                {
                    FramebufferCreateInfo framebufferInfo = new()
                    {
                        SType = StructureType.FramebufferCreateInfo,
                        RenderPass = renderPass.Handle,
                        AttachmentCount = (uint) attachments.Length,
                        PAttachments = attachmentsPtr,
                        Width = _swapChainExtent.Width,
                        Height = _swapChainExtent.Height,
                        Layers = 1,
                    };

                    if (_vk.CreateFramebuffer(
                            _device.LogicalDevice,
                            framebufferInfo,
                            null,
                            out _swapChainFramebuffers[i]
                        ) != Result.Success)
                    {
                        throw new Exception("Failed to create framebuffer!");
                    }
                }
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
                Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange =
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            if (_vk.CreateImageView(
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

    private ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags)
    {
        ImageViewCreateInfo createInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = format,
            Components =
            {
                R = ComponentSwizzle.Identity,
                G = ComponentSwizzle.Identity,
                B = ComponentSwizzle.Identity,
                A = ComponentSwizzle.Identity,
            },
            SubresourceRange =
            {
                AspectMask = aspectFlags,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1,
            }
        };

        if (_vk.CreateImageView(
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

    private struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
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

    private static SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
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

    private static PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
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