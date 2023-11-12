using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace EngineCore.Rendering.Core;

public unsafe partial class VulkanContext
{
    private IWindow? _window;
    private Vk _vk;
    private Instance _instance;

    // Debug
    private ExtDebugUtils? _debugUtils;
    private DebugUtilsMessengerEXT _debugMessenger;

    // Surface 
    private KhrSurface? _khrSurface;
    private SurfaceKHR _surface;

    // Device
    private VulkanDevice _device;
    
    private Swapchain _swapchain;
    // TODO: Group by render pass, for now just embed it
    private RenderPass _renderPass;

    private bool _enableValidationLayers;

    public void Destroy()
    {
        _renderPass.Destroy();
        _swapchain.Destroy(_vk);
    }

    private string[] GetRequiredExtensions(IView window)
    {
        var windowExtensions = window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
        var extensions = SilkMarshal.PtrToStringArray((nint) windowExtensions, (int) glfwExtensionCount);

        if (OperatingSystem.IsMacOS())
        {
            Array.Resize(ref extensions, extensions.Length + 1);
            extensions[^1] = "VK_KHR_portability_enumeration";
        }

        if (_enableValidationLayers)
        {
            return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }

        return extensions;
    }
}