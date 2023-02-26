using Exomia.Vulkan.Api.Core;
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

    public void Initialise(IWindow window)
    {
        _window = window;
        CreateVkInstance();
        CreateSurface();
        PickPhysicalDevice();
        CreateLogicalDevice();
        
        // TODO: createSwapChain
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
        _surface = _window
            .VkSurface!
            .Create<VkAllocationCallbacks>(new VkHandle((nint) (void*) Instance), null);

        if (_surface.Handle == 0)
        {
            throw new Exception($"Failed to create VkSuraface!");
        }
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