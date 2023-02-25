using Exomia.Vulkan.Api.Core;
using Exomia.Vulkan.Api.Screen;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Windowing;
using static Exomia.Vulkan.Api.Core.Vk;

unsafe
{
    var options = WindowOptions.DefaultVulkan with
    {
        Title = "Vulkan Test",
    };
    var window = Window.Create(options);
    window.Initialize();

    // var glfwExtensions = window.VkSurface.GetRequiredExtensions(out var glfwExtensionsCount);
    // var extensions = SilkMarshal.PtrToStringArray((nint) glfwExtensions, (int) glfwExtensionsCount);
    // Array.Resize(ref extensions, extensions.Length + 1);
    // extensions[^1] = "VK_KHR_portability_enumeration";

    var glfwExtensionsPtr = window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
    var glfwExtensions = SilkMarshal.PtrToStringArray((nint) glfwExtensionsPtr, (int) glfwExtensionCount);

    var instanceExtensions = glfwExtensions
        .Append("VK_KHR_portability_enumeration")
        .ToArray();

    var applicationInfo = new VkApplicationInfo
    {
        sType = VkApplicationInfo.STYPE,
        pNext = null,
        pApplicationName = "my app".ToPointer(),
        applicationVersion = new VkVersion(0, 1, 0, 0),
        pEngineName = "my engine".ToPointer(),
        engineVersion = new VkVersion(0, 1, 0, 0),
        apiVersion = VkVersion.VulkanApiVersion12
    };

    var enableLayers = new[]
    {
        "VK_LAYER_KHRONOS_validation",
    };

    var instanceCreateInfo = new VkInstanceCreateInfo
    {
        sType = VkInstanceCreateInfo.STYPE,
        pNext = null,
        flags = VkInstanceCreateFlagBits.VK_INSTANCE_CREATE_ENUMERATE_PORTABILITY_BIT_KHR,
        pApplicationInfo = &applicationInfo,
        enabledLayerCount = (uint) enableLayers.Length,
        ppEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(enableLayers),
        enabledExtensionCount = (uint) instanceExtensions.Length,
        ppEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(instanceExtensions),
    };

    VkInstance instance;
    VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
    Console.WriteLine(result);
    if (result != VkResult.VK_SUCCESS)
        return;

    // Surface
    var surface = window.VkSurface
        .Create<VkAllocationCallbacks>(new VkHandle((nint) (void*) instance), null);

    Debug.Log(surface.ToString());

    uint devicesCount = 0;
    vkEnumeratePhysicalDevices(instance, &devicesCount, null);

    if (devicesCount == 0)
    {
        throw new Exception("failed to find GPUs with Vulkan support!");
    }

    var devices = new VkPhysicalDevice[devicesCount];
    fixed (VkPhysicalDevice* devicesPtr = devices)
    {
        vkEnumeratePhysicalDevices(instance, &devicesCount, devicesPtr);
    }

    var physicalDevice = FindSuitableDevice(devices);

    var vkQueueGraphicsBit = (uint) (VkQueueFlagBits.VK_QUEUE_GRAPHICS_BIT | VkQueueFlagBits.VK_QUEUE_COMPUTE_BIT);
    VkDeviceQueueCreateInfo queueCreateInfo = new()
    {
        sType = VkDeviceQueueCreateInfo.STYPE,
        queueFamilyIndex = vkQueueGraphicsBit,
        queueCount = 1
    };

    float queuePriority = 1.0f;
    queueCreateInfo.pQueuePriorities = &queuePriority;

    VkPhysicalDeviceFeatures deviceFeatures = new();

    var createInfo = new VkDeviceCreateInfo()
    {
        sType = VkDeviceCreateInfo.STYPE,
        queueCreateInfoCount = 1,
        pQueueCreateInfos = &queueCreateInfo,

        pEnabledFeatures = &deviceFeatures,

        enabledExtensionCount = 0,
        enabledLayerCount = 0
    };

    VkDevice device = new VkDevice();
    if (vkCreateDevice(physicalDevice!.Value, &createInfo, null, &device) != VkResult.VK_SUCCESS)
    {
        throw new Exception("failed to create logical device!");
    }

    VkQueue graphicsQueue = new VkQueue();
    vkGetDeviceQueue(device, vkQueueGraphicsBit, 0, &graphicsQueue);

    window.Run(() =>
    {
        
    });
}

unsafe VkPhysicalDevice? FindSuitableDevice(VkPhysicalDevice[] vkPhysicalDevices)
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