using System.Diagnostics;
using MtgWeb.Core;
using Silk.NET.Maths;

namespace EngineCore;

using Silk.NET.Windowing;

// TODO: Make 'Application' Layer on top of CoreLoop as entry point and
// host other user code libs.
public class CoreLoop : IDisposable
{
    private readonly WindowModule _windowModule;
    private readonly RenderModule _renderModule;
    private readonly ScriptingModule _scriptingModule;
    private Stopwatch _stopwatch;
    private Scene _currentScene;
    private Input.Bridge _inputBridge;

    public CoreLoop()
    {
        _stopwatch = new Stopwatch();

        _windowModule = new WindowModule();
        _windowModule.Init();

        _renderModule = new RenderModule(_windowModule.Window);
        _renderModule.InitVulkan();

        _scriptingModule = new ScriptingModule();

        _windowModule.Window.Render += MainLoop;
        
        _inputBridge = new Input.Bridge();
        _inputBridge.Bind(_windowModule.Window);

        Initialise();
    }

    public void Run()
    {
        _windowModule.Window.Run();
        _renderModule.DeviceWaitIdle();
    }

    private void Initialise()
    {
        _currentScene = new Scene();
        var cameraEntity = new Entity()
        {
            Name = "Camera",
        };
        cameraEntity.Transform.Matrix = Matrix4X4<float>.Identity * Matrix4X4.CreateLookAt(
            new Vector3D<float>(1, 1, 1),
            new Vector3D<float>(0, 0, 0),
            new Vector3D<float>(0, 0, 1)
        );

        var camera = cameraEntity.AddComponent<Camera>();
        cameraEntity.AddComponent<FreeCamera>()
            .Inject(camera);

        _currentScene.Root = new[]
        {
            cameraEntity
        };
        
        foreach (var entity in _currentScene.Root)
        {
            entity.BindHierarchy();
            entity.InitComponents();
        }

        foreach (var entity in _currentScene.Root)
        {
            entity.StartComponents();
        }
    }

    private void MainLoop(double delta)
    {
        _stopwatch.Stop();
        Time.StartFrame(_stopwatch.ElapsedMilliseconds * 0.001f);
        _stopwatch.Restart();

        Input.Update();
        SceneUpdate();
        Input.LateUpdate();

        _renderModule.DrawFrame(_currentScene, delta);

        Time.EndFrame(_stopwatch);
    }

    private void SceneUpdate()
    {
        foreach (var entity in _currentScene.Root)
        {
            if (entity.Enabled)
            {
                entity.UpdateComponents();
                entity.Transform.Update();
            }
        }
    }

    public void Dispose()
    {
        _currentScene.Dispose();
        _scriptingModule.Dispose();
        _inputBridge.Dispose();
        _renderModule.Dispose();
    }
}

internal class ScriptingModule : IDisposable
{
    public void Update() { }

    public void Dispose() { }
}