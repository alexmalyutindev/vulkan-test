using System;
using System.Linq;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Render
{
    public unsafe class Renderer
    {
        private static IWindow? _window;
        private static Vk? _vk;

        private Instance _instance;
        private PhysicalDevice _physicalDevice;
        private Device _device;

        private Queue graphicsQueue;
        private KhrSurface _khrSurface;
        private SurfaceKHR _surface;

        private readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation"
        };

        public void Init()
        {
            CreateWindow();
            InitVulkan();
        }

        private static void CreateWindow()
        {
            var options = WindowOptions.DefaultVulkan with
            {
                Title = "Vulkan Test"
            };
            _window = Window.Create(options);
            _window.Initialize();
        }

        private void InitVulkan()
        {
            Console.WriteLine("Initialising Vulkan...");
            CreateInstance();
            CreateSurface();
            PickPhysicalDevice();
            CreateLogicalDevice();
        }

        private void CreateInstance()
        {
            _vk = Vk.GetApi();

            if (!CheckValidationLayerSupport())
            {
                Console.WriteLine("Validation not supported!");
            }

            var appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = "Vulkan Test".ToPointer(),
                ApplicationVersion = Vk.MakeVersion(1, 0, 0),
                PEngineName = "Engine".ToPointer(),
                EngineVersion = Vk.MakeVersion(0, 0, 1),
                ApiVersion = Vk.Version13,
            };

            var extensions = GetRequiredExtensions();
            var createInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint) extensions.Length,
                PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(extensions),
            };

            var result = _vk.CreateInstance(createInfo, null, out _instance);
            Console.WriteLine($"Create Instance: {result}\nWith ext:\n - {String.Join("\n - ", extensions)}");
        }

        private bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;
            _vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
            var availableLayers = new LayerProperties[layerCount];
            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                _vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }

            var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr) layer.LayerName))
                .ToHashSet();

            return validationLayers.All(availableLayerNames.Contains);
        }

        private void CreateSurface()
        {
            if (!_vk!.TryGetInstanceExtension<KhrSurface>(_instance, out _khrSurface))
            {
                throw new NotSupportedException("KHR_surface extension not found.");
            }

            var callback = new AllocationCallbacks();
            _surface = _window!.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), &callback).ToSurface();
        }

        private void PickPhysicalDevice()
        {
            uint devicesCount = 0;
            _vk!.EnumeratePhysicalDevices(_instance, ref devicesCount, null);

            if (devicesCount == 0)
            {
                throw new Exception("failed to find GPUs with Vulkan support!");
            }

            var devices = new PhysicalDevice[devicesCount];
            fixed (PhysicalDevice* devicesPtr = devices)
            {
                _vk!.EnumeratePhysicalDevices(_instance, ref devicesCount, devicesPtr);
            }

            foreach (var device in devices)
            {
                if (IsDeviceSuitable(device))
                {
                    _physicalDevice = device;
                    break;
                }
            }

            if (_physicalDevice.Handle == 0)
            {
                throw new Exception("failed to find a suitable GPU!");
            }
        }

        private void CreateLogicalDevice()
        {
            var indices = FindQueueFamilies(_physicalDevice);

            DeviceQueueCreateInfo queueCreateInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = indices.GraphicsFamily!.Value,
                QueueCount = 1
            };

            float queuePriority = 1.0f;
            queueCreateInfo.PQueuePriorities = &queuePriority;

            PhysicalDeviceFeatures deviceFeatures = new();

            var createInfo = new DeviceCreateInfo()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,

                PEnabledFeatures = &deviceFeatures,

                EnabledExtensionCount = 0,
                EnabledLayerCount = 0
            };

            if (_vk!.CreateDevice(_physicalDevice, in createInfo, null, out _device) != Result.Success)
            {
                throw new Exception("failed to create logical device!");
            }

            _vk!.GetDeviceQueue(_device, indices.GraphicsFamily!.Value, 0, out graphicsQueue);
        }

        private string[] GetRequiredExtensions()
        {
            var glfwExtensions = _window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
            var extensions = SilkMarshal.PtrToStringArray((nint) glfwExtensions, (int) glfwExtensionCount);

            return extensions;
        }

        private bool IsDeviceSuitable(PhysicalDevice device)
        {
            var indices = FindQueueFamilies(device);

            return indices.IsComplete();
        }

        private QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
        {
            var indices = new QueueFamilyIndices();

            uint queueFamilityCount = 0;
            _vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

            var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
            fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
            {
                _vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
            }

            uint i = 0;
            foreach (var queueFamily in queueFamilies)
            {
                if (queueFamily.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    indices.GraphicsFamily = i;
                }

                if (indices.IsComplete())
                {
                    break;
                }

                i++;
            }

            return indices;
        }
        
        private static string[] EnumerateInstanceLayers()
        {
            uint count = 0;
            var result = _vk.EnumerateInstanceLayerProperties(ref count, null);
            if (result != Result.Success)
            {
                return Array.Empty<string>();
            }

            if (count == 0)
            {
                return Array.Empty<string>();
            }

            LayerProperties* properties = stackalloc LayerProperties[(int)count];
            _vk.EnumerateInstanceLayerProperties(&count, properties).CheckResult();

            string[] resultExt = new string[count];
            for (int i = 0; i < count; i++)
            {
                resultExt[i] = properties[i].GetLayerName();
            }

            return resultExt;
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