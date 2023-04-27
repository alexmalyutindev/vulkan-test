using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace RenderCore.RenderModule;

public unsafe partial class VulkanContext
{
    struct QueueFamilyIndices
    {
        public uint? GraphicsFamily { get; set; }
        public uint? PresentFamily { get; set; }

        public bool IsComplete()
        {
            return GraphicsFamily.HasValue && PresentFamily.HasValue;
        }
    }

    private readonly string[] _deviceExtensions = new[]
    {
        KhrSwapchain.ExtensionName
    };

    private void CreateLogicalDevice()
    {
        var indices = FindQueueFamilies(_device.PhysicalDevice);

        var uniqueQueueFamilies = new[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };
        uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

        using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*) Unsafe.AsPointer(ref mem.GetPinnableReference());

        float queuePriority = 1.0f;
        for (int i = 0; i < uniqueQueueFamilies.Length; i++)
        {
            queueCreateInfos[i] = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilies[i],
                QueueCount = 1
            };


            queueCreateInfos[i].PQueuePriorities = &queuePriority;
        }

        PhysicalDeviceFeatures deviceFeatures = new()
        {
            SamplerAnisotropy = true,
        };


        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint) uniqueQueueFamilies.Length,
            PQueueCreateInfos = queueCreateInfos,

            PEnabledFeatures = &deviceFeatures,

            EnabledExtensionCount = (uint) _deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(_deviceExtensions)
        };

        if (_enableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint) _validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(_validationLayers);
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
        }

        if (_vk!.CreateDevice(_device.PhysicalDevice, in createInfo, null, out _device.LogicalDevice) != Result.Success)
        {
            throw new Exception("failed to create logical device!");
        }

        _vk!.GetDeviceQueue(_device.LogicalDevice, indices.GraphicsFamily!.Value, 0, out _device.GraphicsQueue);
        _vk!.GetDeviceQueue(_device.LogicalDevice, indices.PresentFamily!.Value, 0, out _device.PresentQueue);

        if (_enableValidationLayers)
        {
            SilkMarshal.Free((nint) createInfo.PpEnabledLayerNames);
        }

        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);
    }

    private void PickPhysicalDevice()
    {
        uint devicedCount = 0;
        _vk!.EnumeratePhysicalDevices(_instance, ref devicedCount, null);

        if (devicedCount == 0)
        {
            throw new Exception("failed to find GPUs with Vulkan support!");
        }

        var devices = new PhysicalDevice[devicedCount];
        fixed (PhysicalDevice* devicesPtr = devices)
        {
            _vk!.EnumeratePhysicalDevices(_instance, ref devicedCount, devicesPtr);
        }

        foreach (var device in devices)
        {
            if (IsDeviceSuitable(device))
            {
                _device.PhysicalDevice = device;
                break;
            }
        }

        if (_device.PhysicalDevice.Handle == 0)
        {
            throw new Exception("failed to find a suitable GPU!");
        }
    }

    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        var indices = FindQueueFamilies(device);

        bool extensionsSupported = CheckDeviceExtensionsSupport(device);

        bool swapChainAdequate = false;
        if (extensionsSupported)
        {
            var swapChainSupport = QuerySwapChainSupport(device);
            swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
        }

        _vk!.GetPhysicalDeviceFeatures(device, out var supportedFeatures);

        return indices.IsComplete() && extensionsSupported && swapChainAdequate && supportedFeatures.SamplerAnisotropy;
    }
    
    private bool CheckDeviceExtensionsSupport(PhysicalDevice device)
    {
        uint extentionsCount = 0;
        _vk!.EnumerateDeviceExtensionProperties(device, (byte*) null, ref extentionsCount, null);

        var availableExtensions = new ExtensionProperties[extentionsCount];
        fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
        {
            _vk!.EnumerateDeviceExtensionProperties(device, (byte*) null, ref extentionsCount, availableExtensionsPtr);
        }

        var availableExtensionNames = availableExtensions
            .Select(extension => Marshal.PtrToStringAnsi((IntPtr) extension.ExtensionName)).ToHashSet();

        return _deviceExtensions.All(availableExtensionNames.Contains);
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
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                indices.GraphicsFamily = i;
            }

            _khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, _surface, out var presentSupport);

            if (presentSupport)
            {
                indices.PresentFamily = i;
            }

            if (indices.IsComplete())
            {
                break;
            }

            i++;
        }

        return indices;
    }
}