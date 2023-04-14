namespace RenderCore;
using Silk.NET.Windowing;

public class CoreLoop : IDisposable
{
    private WindowModule _windowModule;
    private VulkanContext _renderModule;

    public CoreLoop()
    {
        _windowModule = new WindowModule();
        _windowModule.Init();

        _renderModule = new VulkanContext(_windowModule.Window);
        _renderModule.InitVulkan();

        _windowModule.Window.Render += MainLoop;

        _windowModule.Window.Run();
        _renderModule.DeviceWaitIdle();
    }

    private void MainLoop(double delta)
    {
        _renderModule.DrawFrame(delta);
    }

    public void Dispose()
    {
        _renderModule.CleanUp();
    }
}