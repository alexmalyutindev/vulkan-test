using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace EngineCore.Rendering.Core;

public unsafe partial class VulkanContext
{
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
    }

    private void CreateInstance()
    {
        _vk = Vk.GetApi();

        if (_enableValidationLayers && !CheckValidationLayerSupport())
        {
            throw new Exception("validation layers requested, but not available!");
        }

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi("Hello Triangle"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("No Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        if (OperatingSystem.IsMacOS())
        {
            createInfo.Flags = InstanceCreateFlags.EnumeratePortabilityBitKhr;
        }

        var extensions = GetRequiredExtensions(_window);
        createInfo.EnabledExtensionCount = (uint) extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(extensions);

        if (_enableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint) _validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(_validationLayers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }

        if (_vk.CreateInstance(createInfo, null, out _instance) != Result.Success)
        {
            throw new Exception("failed to create instance!");
        }

        Marshal.FreeHGlobal((IntPtr) appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr) appInfo.PEngineName);
        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);

        if (_enableValidationLayers)
        {
            SilkMarshal.Free((nint) createInfo.PpEnabledLayerNames);
        }
    }

    private void CreateSurface()
    {
        if (!_vk!.TryGetInstanceExtension<KhrSurface>(_instance, out _khrSurface))
        {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        _surface = _window!.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();
    }
}