using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RenderCore;

public class WindowModule
{
    const int Width = 800;
    const int Height = 600;

    public IWindow? Window;

    public void Init()
    {
        var options = WindowOptions.DefaultVulkan with
        {
            Size = new Vector2D<int>(Width, Height),
            Title = "Vulkan",
        };

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Initialize();

        if (Window.VkSurface is null)
        {
            throw new Exception("Windowing platform doesn't support Vulkan.");
        }
    }
}