using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace EngineCore.Rendering.Core;

public sealed unsafe partial class VulkanContext
{
    private IWindow? _window;
    private Vk _vk;
    private Instance _instance;

    // Debug
    private ExtDebugUtils? _debugUtils;
    private DebugUtilsMessengerEXT _debugMessenger;
    private bool _enableValidationLayers;

    // Surface 
    private KhrSurface? _khrSurface;
    private SurfaceKHR _surface;

    // Device
    private VulkanDevice _device;
    
    private Swapchain _swapchain;
    // TODO: Group by render pass, for now just embed it
    private RenderPass _renderPass;

    public void InitVulkan(IWindow window)
    {
        _window = window;
        CreateInstance();
        SetupDebugMessenger();
        CreateSurface();

        _device = new VulkanDevice(this);
        PickPhysicalDevice();
        CreateLogicalDevice();

        _swapchain = new Swapchain(this);
        _swapchain.CreateSwapChain();
        {
            _swapchain.CreateImageViews();
            _swapchain.CreateDepthResources();
        }

        // TODO: Make configurable
        _renderPass = new RenderPass(this);
        _renderPass.CreateRenderPass(_swapchain.Format);
        _swapchain.CreateFramebuffers(_renderPass);

        _graphicsPipeline = new GraphicsPipeline(this);

        _graphicsPipeline.CreateDescriptorSetLayout();
        _graphicsPipeline.CreateGraphicsPipeline(
            _renderPass,
            "shaders/vert.spv",
            "shaders/frag.spv",
            _swapchain.Extent
        );
    }

    public void Render()
    {
        _swapchain.Present();
    }

    public void Destroy()
    {
        _renderPass.Destroy();
        _swapchain.Destroy();
        _graphicsPipeline.Destroy();
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