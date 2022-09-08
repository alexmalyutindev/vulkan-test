using Render;
using Silk.NET.Windowing;

public class Program
{
    private static IWindow? _window;

    public static void Main()
    {
        _window = CreateWindow();
        
        var renderer = new Renderer();
        renderer.Init(_window);
        
        _window.Run();
    }
    
    private static IWindow CreateWindow()
    {
        var options = WindowOptions.DefaultVulkan with
        {
            Title = "Vulkan Test",
        };
        var window = Window.Create(options);
        window.Initialize();
        return window;
    }
}