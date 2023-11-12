using System.Diagnostics;
using EngineCore.Rendering.Core;
using MtgWeb.Core;
using Silk.NET.Maths;

namespace EngineCore;

using Silk.NET.Windowing;

// TODO: Make 'Application' Layer on top of CoreLoop as entry point and
// host other user code libs.
public class CoreLoop : IDisposable
{
    private readonly WindowModule _windowModule;
    private readonly ExampleRenderer _exampleRenderer;
    private readonly ScriptingModule _scriptingModule;
    private Stopwatch _stopwatch;
    private Scene _currentScene;
    private Input.Bridge _inputBridge;

    public CoreLoop()
    {
        _stopwatch = new Stopwatch();

        _windowModule = new WindowModule();
        _windowModule.Init();

        _exampleRenderer = new ExampleRenderer(_windowModule.Window);
        _exampleRenderer.InitVulkan();

        _scriptingModule = new ScriptingModule();

        _windowModule.Window.Render += MainLoop;
        
        _inputBridge = new Input.Bridge();
        _inputBridge.Bind(_windowModule.Window);

        Initialise();
    }

    public void Run()
    {
        _windowModule.Window.Run();
        _exampleRenderer.DeviceWaitIdle();
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

        _exampleRenderer.DrawFrame(_currentScene, delta);

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
        _exampleRenderer.Dispose();
    }
}

internal class ScriptingModule : IDisposable
{
    public void Update() { }

    public void Dispose() { }
}