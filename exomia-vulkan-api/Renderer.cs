using Exomia.Vulkan.Api.Core;
using Silk.NET.Core.Native;
using static Exomia.Vulkan.Api.Core.Vk;

namespace Render
{
    public unsafe class Renderer
    {
        public void InitVulkan()
        {
            Debug.Log("Initialising Vulkan...");
            CreateInstance();
        }

        private void CreateInstance()
        {
            var appInfo = new VkApplicationInfo()
            {
                sType = VkApplicationInfo.STYPE,
                pNext = null,
                pApplicationName = "Vulkan Test".ToPointer(),
                applicationVersion = new VkVersion(0, 1, 0, 0),
                pEngineName = "Engine".ToPointer(),
                engineVersion = new VkVersion(0, 0, 0, 1),
                apiVersion = VkVersion.VulkanApiVersion12,
            };

            var extensions = new []
            {
                "VK_KHR_portability_enumeration"
            };
            var instanceCreateInfo = new VkInstanceCreateInfo()
            {
                sType = VkInstanceCreateInfo.STYPE,
                pNext = null,
                flags = VkInstanceCreateFlagBits.VK_INSTANCE_CREATE_ENUMERATE_PORTABILITY_BIT_KHR,
                pApplicationInfo = &appInfo,
                enabledExtensionCount = (uint) extensions.Length,
                ppEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(extensions),
                enabledLayerCount = 1,
                ppEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(new[] { "VK_LAYER_KHRONOS_validation" }),
            };

            VkInstance instance;
            VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
            Console.WriteLine(result);
            Debug.Log($"Create Instance: {result}\n" +
                      $" With ext:\n" +
                      $"  - {String.Join<string>("\n  - ", extensions)}");
        }
    }
}

struct QueueFamilyIndices
{
    public uint? GraphicsFamily { get; set; }

    public bool IsComplete()
    {
        return GraphicsFamily.HasValue;
    }
}