using Exomia.Vulkan.Api.Core;
using Silk.NET.Core.Native;
using static Exomia.Vulkan.Api.Core.Vk;

unsafe
{
    var applicationInfo = new VkApplicationInfo
    {
        sType = VkApplicationInfo.STYPE,
        pNext = null,
        pApplicationName = "my app".ToPointer(),
        applicationVersion = new VkVersion(0, 1, 0, 0),
        pEngineName = "my engine".ToPointer(),
        engineVersion = new VkVersion(0, 1, 0, 0),
        apiVersion = VkVersion.VulkanApiVersion13
    };

    
    var instanceCreateInfo = new VkInstanceCreateInfo
    {
        sType = VkInstanceCreateInfo.STYPE,
        pNext = null,
        flags = VkInstanceCreateFlagBits.VK_INSTANCE_CREATE_ENUMERATE_PORTABILITY_BIT_KHR,
        pApplicationInfo = &applicationInfo,
        enabledLayerCount = 2u,
        ppEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(new[]
        {
            "VK_KHR_portability_enumeration",
            "VK_LAYER_KHRONOS_validation"
        }),
        enabledExtensionCount = 0u,
        ppEnabledExtensionNames = null
    };

    VkInstance instance;
    VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
    Console.WriteLine(result);
}