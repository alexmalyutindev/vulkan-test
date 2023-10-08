using Silk.NET.Vulkan;

namespace RenderCore.RenderModule;

public partial class VulkanContext
{
    private Format FindDepthFormat()
    {
        return FindSupportedFormat(
            new[] { Format.D32Sfloat, Format.D32SfloatS8Uint, Format.D24UnormS8Uint },
            ImageTiling.Optimal,
            FormatFeatureFlags.DepthStencilAttachmentBit
        );
    }
        
    private Format FindSupportedFormat(
        IEnumerable<Format> candidates,
        ImageTiling tiling,
        FormatFeatureFlags features
    )
    {
        foreach (var format in candidates)
        {
            _vk!.GetPhysicalDeviceFormatProperties(_device.PhysicalDevice, format, out var props);

            if (tiling == ImageTiling.Linear && (props.LinearTilingFeatures & features) == features)
            {
                return format;
            }
            else if (tiling == ImageTiling.Optimal && (props.OptimalTilingFeatures & features) == features)
            {
                return format;
            }
        }

        throw new Exception("failed to find supported format!");
    }
}