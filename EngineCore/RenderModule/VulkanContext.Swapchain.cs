using Silk.NET.Vulkan;
using Image = Silk.NET.Vulkan.Image;

namespace RenderCore.RenderModule;

public unsafe partial class VulkanContext
{
    private struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }

    private void CreateSwapChain()
    {
        var swapChainSupport = QuerySwapChainSupport(_device.PhysicalDevice);

        var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
        var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

        var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
        {
            imageCount = swapChainSupport.Capabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR creatInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,

            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
        };

        var indices = FindQueueFamilies(_device.PhysicalDevice);
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

        if (_swapchain._khrSwapChain is null)
        {
            if (!_vk!.TryGetDeviceExtension(_instance, _device.LogicalDevice, out _swapchain._khrSwapChain))
            {
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            }
        }

        if (_swapchain._khrSwapChain!.CreateSwapchain(
                _device.LogicalDevice,
                creatInfo,
                null,
                out _swapchain._swapChain
            ) != Result.Success)
        {
            throw new Exception("failed to create swap chain!");
        }

        _swapchain._khrSwapChain.GetSwapchainImages(_device.LogicalDevice, _swapchain._swapChain, ref imageCount, null);
        _swapchain._swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = _swapchain._swapChainImages)
        {
            _swapchain._khrSwapChain.GetSwapchainImages(_device.LogicalDevice, _swapchain._swapChain, ref imageCount, swapChainImagesPtr);
        }

        _swapchain._swapChainImageFormat = surfaceFormat.Format;
        _swapchain._swapChainExtent = extent;
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