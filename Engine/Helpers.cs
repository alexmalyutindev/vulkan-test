using System.Diagnostics;
using Exomia.Vulkan.Api.Core;

namespace Engine;

public static unsafe class Helpers
{
    public static byte* ToPointer(this string text)
    {
        return (byte*) System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(text);
    }

    public static uint Version(uint major, uint minor, uint patch)
    {
        return (major << 22) | (minor << 12) | patch;
    }

    public static string GetString(byte* stringStart)
    {
        int characters = 0;
        while (stringStart[characters] != 0)
        {
            characters++;
        }

        return System.Text.Encoding.UTF8.GetString(stringStart, characters);
    }

    [DebuggerHidden]
    [DebuggerStepThrough]
    public static void CheckResult(this VkResult result, string message = "Vulkan error occured")
    {
        if (result != VkResult.VK_SUCCESS)
        {
            throw new Exception(message);
        }
    }

    // public static string GetLayerName(this LayerProperties properties)
    // {
    //     return GetString(properties.LayerName);
    // }
}