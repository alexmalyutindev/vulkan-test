using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace EngineCore;

public class WindowModule
{
    const int Width = 800;
    const int Height = 600;

    public IWindow Window => _window!;
    private IWindow? _window;

    public void Init()
    {
        var options = WindowOptions.DefaultVulkan with
        {
            Size = new Vector2D<int>(Width, Height),
            Title = "Vulkan",
        };

        _window = Silk.NET.Windowing.Window.Create(options);
        _window.Initialize();

        if (_window.VkSurface is null)
        {
            throw new Exception("Windowing platform doesn't support Vulkan.");
        }
    }
}