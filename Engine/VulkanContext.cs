using Exomia.Vulkan.Api.Core;
using Exomia.Vulkan.Api.Metal;
using Silk.NET.Core.Native;
using Silk.NET.Windowing;
using static Exomia.Vulkan.Api.Core.Vk;

namespace Engine.Rendering;

public unsafe class VulkanContext
{
    public VkInstance Instance;

    private IWindow _window;
    private VkNonDispatchableHandle _surface;
    private VkPhysicalDevice _physicalDevice;
    private VkDevice _device;
    private uint _vkQueueGraphicsBit;
    private VkSurfaceKHR _vkSurfaceKhr;

    public void Initialise(IWindow window)
    {
        _window = window;
        CreateVkInstance();
        CreateSurface();
        PickPhysicalDevice();
        CreateLogicalDevice();
        // TODO: createSwapChain
        CreateSwapChain();

        // TODO: createImageViews
        // TODO: createRenderPass
        // TODO: createGraphicsPipeline
        // TODO: createFramebuffers

        // TODO: Refactoring
        {
            var graphicsQueue = new VkQueue();
            vkGetDeviceQueue(_device, _vkQueueGraphicsBit, 0, &graphicsQueue);

            var vkCommandPool = new VkCommandPool();
            var vkCommandPoolCreateInfo = new VkCommandPoolCreateInfo()
            {
                sType = VkCommandPoolCreateInfo.STYPE,
                pNext = null,
                flags = VkCommandPoolCreateFlagBits.VK_COMMAND_POOL_CREATE_TRANSIENT_BIT,
                queueFamilyIndex = _vkQueueGraphicsBit
            };
            if (vkCreateCommandPool(_device, &vkCommandPoolCreateInfo, null, &vkCommandPool) != VkResult.VK_SUCCESS)
            {
                throw new Exception($"Failed to {nameof(vkCreateCommandPool)}!");
            }

            var cmd = new VkCommandBuffer();
            var vkCommandBufferAllocateInfo = new VkCommandBufferAllocateInfo()
            {
                sType = VkCommandBufferAllocateInfo.STYPE,
                level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY,
                commandPool = vkCommandPool,
                commandBufferCount = 1
            };

            if (vkAllocateCommandBuffers(_device, &vkCommandBufferAllocateInfo, &cmd) != VkResult.VK_SUCCESS)
            {
                throw new Exception($"Failed to {nameof(vkAllocateCommandBuffers)}!");
            }
        }
    }

    private void CreateVkInstance()
    {
        var windowExtensionsPtr = _window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
        var windowExtensions = SilkMarshal.PtrToStringArray((nint) windowExtensionsPtr, (int) glfwExtensionCount);

        var instanceExtensions = windowExtensions
#if VK_PORTABILITY
            .Append("VK_KHR_portability_enumeration")
#endif
            .ToArray();

        var applicationInfo = new VkApplicationInfo
        {
            sType = VkApplicationInfo.STYPE,
            pNext = null,
            pApplicationName = "Game".ToPointer(),
            applicationVersion = new VkVersion(0, 1, 0, 0),
            pEngineName = "Engine".ToPointer(),
            engineVersion = new VkVersion(0, 1, 0, 0),
            apiVersion = VkVersion.VulkanApiVersion12
        };

        var enableLayers = new string[]
        {
#if DEBUG
            "VK_LAYER_KHRONOS_validation",
#endif
        };

        var instanceCreateInfo = new VkInstanceCreateInfo
        {
            sType = VkInstanceCreateInfo.STYPE,
            pNext = null,
#if VK_PORTABILITY
            flags = VkInstanceCreateFlagBits.VK_INSTANCE_CREATE_ENUMERATE_PORTABILITY_BIT_KHR,
#endif
            pApplicationInfo = &applicationInfo,
            enabledLayerCount = (uint) enableLayers.Length,
            ppEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(enableLayers),
            enabledExtensionCount = (uint) instanceExtensions.Length,
            ppEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(instanceExtensions),
        };

        VkInstance instance;
        if (vkCreateInstance(&instanceCreateInfo, null, &instance) != VkResult.VK_SUCCESS)
        {
            throw new Exception($"Failed to {nameof(vkCreateInstance)}!");
        }

        Instance = instance;
    }

    private void CreateSurface()
    {
        var vkSurfaceKhr = new VkSurfaceKHR();
#if METAL
        VkExtMetalSurface.Load(Instance);
        var vkMetalSurfaceCreateInfoExt = new VkMetalSurfaceCreateInfoEXT()
        {
            sType = VkMetalSurfaceCreateInfoEXT.STYPE,
            pNext = null,
            flags = 0
        };
        if (VkExtMetalSurface.vkCreateMetalSurfaceEXT(
                Instance,
                &vkMetalSurfaceCreateInfoExt,
                null,
                &vkSurfaceKhr
            ) != VkResult.VK_SUCCESS)
        {
            throw new Exception($"Failed to create VkSuraface!");
        }
#endif
        _vkSurfaceKhr = vkSurfaceKhr;
    }

    private void PickPhysicalDevice()
    {
        uint devicesCount = 0;
        vkEnumeratePhysicalDevices(Instance, &devicesCount, null);

        if (devicesCount == 0)
        {
            throw new Exception("failed to find GPUs with Vulkan support!");
        }

        var devices = new VkPhysicalDevice[devicesCount];
        fixed (VkPhysicalDevice* devicesPtr = devices)
        {
            vkEnumeratePhysicalDevices(Instance, &devicesCount, devicesPtr);
        }

        _physicalDevice = FindSuitableDevice(devices);
    }

    private void CreateLogicalDevice()
    {
        _vkQueueGraphicsBit = (uint) (VkQueueFlagBits.VK_QUEUE_GRAPHICS_BIT | VkQueueFlagBits.VK_QUEUE_COMPUTE_BIT);
        VkDeviceQueueCreateInfo queueCreateInfo = new()
        {
            sType = VkDeviceQueueCreateInfo.STYPE,
            queueFamilyIndex = _vkQueueGraphicsBit,
            queueCount = 1
        };

        float queuePriority = 1.0f;
        queueCreateInfo.pQueuePriorities = &queuePriority;

        VkPhysicalDeviceFeatures deviceFeatures = new();

        var deviceExtensions = new string[]
        {
#if VK_PORTABILITY
            "VK_KHR_portability_subset"
#endif
        };
        var createInfo = new VkDeviceCreateInfo()
        {
            sType = VkDeviceCreateInfo.STYPE,
            queueCreateInfoCount = 1,
            pQueueCreateInfos = &queueCreateInfo,
            pEnabledFeatures = &deviceFeatures,
            enabledExtensionCount = (uint) deviceExtensions.Length,
            ppEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(deviceExtensions),
            enabledLayerCount = 0
        };

        var device = new VkDevice();
        if (vkCreateDevice(_physicalDevice, &createInfo, null, &device) != VkResult.VK_SUCCESS)
        {
            throw new Exception("failed to create logical device!");
        }

        _device = device;
    }

    struct SwapChainBuffer {
        VkImage image;
        VkImageView view;
    };

    private void CreateSwapChain()
    {
        VkKhrSwapchain.Load(Instance);
        VkKhrSwapchain.Load(_device);
        var vkSwapchainKhr = new VkSwapchainKHR();

        var vkSwapchainCreateInfoKhr = new VkSwapchainCreateInfoKHR()
        {
            sType = VkSwapchainCreateInfoKHR.STYPE,
            surface = _vkSurfaceKhr,
            minImageCount = 2,
            imageFormat = VkFormat.VK_FORMAT_R16G16B16A16_UINT,
            imageColorSpace = VkColorSpaceKHR.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR,
            imageExtent = new VkExtent2D(){ width = (uint) _window.Size[0], height = (uint) _window.Size[1]},
            imageUsage = VkImageUsageFlagBits.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT,
            preTransform = VkSurfaceTransformFlagBitsKHR.VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR,
            imageArrayLayers = 1,
            imageSharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE,
            queueFamilyIndexCount = 0,
            pQueueFamilyIndices = null,
            presentMode = VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR,
            clipped = true,
            compositeAlpha = VkCompositeAlphaFlagBitsKHR.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR,
        };
        if (VkKhrSwapchain.vkCreateSwapchainKHR(_device, &vkSwapchainCreateInfoKhr, null, &vkSwapchainKhr) !=
            VkResult.VK_SUCCESS)
        {
            throw new Exception("Failed to create swapchain!");
        }
    }

    private VkPhysicalDevice FindSuitableDevice(VkPhysicalDevice[] vkPhysicalDevices)
    {
        foreach (var device in vkPhysicalDevices)
        {
            uint queueFamilityCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilityCount, null);

            var queueFamilies = new VkQueueFamilyProperties[queueFamilityCount];
            fixed (VkQueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilityCount, queueFamiliesPtr);
            }


            foreach (var queueFamily in queueFamilies)
            {
                if ((queueFamily.queueFlags & VkQueueFlagBits.VK_QUEUE_GRAPHICS_BIT) != 0)
                {
                    return device;
                }
            }
        }

        throw new Exception("Failed to find a suitable GPU!");
    }
}