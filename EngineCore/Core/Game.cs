using System.Diagnostics;
using MtgWeb.Core.Physics;
using MtgWeb.Core.Render;
using Mesh = MtgWeb.Core.Render.Mesh;

namespace MtgWeb.Core;

public class Game : IDisposable
{
    // private readonly WebGLContext _context;
    private readonly PhysicsWorld _physicsWorld;

    private Scene? _currentScene;

    private Stopwatch _stopwatch;

    // Temp
    private Camera? _camera;
    private readonly Mesh _quad = Mesh.Quad();
    private readonly Mesh _cube = Mesh.Cube();

    public Game( /* WebGLContext context */)
    {
        // _context = context;
        _physicsWorld = new PhysicsWorld();

        _stopwatch = new Stopwatch();
    }

    public async Task Init()
    {
        await LoadScene("Scene");

        _currentScene!.Root
            .First(entity => entity.Name.Contains("Player"))
            .TryGetComponent(out _camera);

        // await _quad.Init(_context);
        // await _cube.Init(_context);
    }

    private async Task LoadScene(string name)
    {
        _currentScene = await Scene.Load(name);
        // await Shader.CompileAll(_context);
        _physicsWorld.Add(_currentScene);

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

    public async Task MainLoop()
    {
        _stopwatch.Stop();
        Time.StartFrame(_stopwatch.ElapsedMilliseconds * 0.001f);
        _stopwatch.Restart();

        // TODO: Separate Physics loop
        // _physicsWorld.Simulation.Timestep(1.0f / 60f);

        Input.Update();
        await Update();
        Input.LateUpdate();
        await Render();

        Time.EndFrame(_stopwatch);
    }

    private async Task Update()
    {
        foreach (var entity in _currentScene!.Root)
        {
            if (entity.Enabled)
                entity.UpdateComponents();
        }
    }


    struct RenderData : IComparable<RenderData>
    {
        public Entity Entity;
        public Mesh Mesh;
        public Shader? Shader;

        public void Deconstruct(out Entity entity, out Mesh mesh, out Shader? shader)
        {
            entity = Entity;
            mesh = Mesh;
            shader = Shader;
        }

        public int CompareTo(RenderData other)
        {
            return Shader.Queue - other.Shader.Queue;
        }
    }

    private RenderData[] _renderData = new RenderData[256];

    private async Task Render()
    {
#if false
        foreach (var entity in _currentScene.Root)
        {
            entity.Transform.Update();
            foreach (var child in entity.Children) // TODO
            {
                child.Transform.Update();
            }
        }

        var clearColor = _camera.ClearColor;
        await _context.BeginBatchAsync();
        {
            await _context.ViewportAsync(0, 0, MainView.Width, MainView.Height);
            await _context.ClearColorAsync(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            await _context.ClearAsync(BufferBits.COLOR_BUFFER_BIT);

            await _context.EnableAsync(EnableCap.CULL_FACE);
            await _context.EnableAsync(EnableCap.DEPTH_TEST);

            await _context.EnableAsync(EnableCap.BLEND);
            await _context.BlendEquationAsync(BlendingEquation.FUNC_ADD);
            await _context.BlendFuncAsync(BlendingMode.SRC_ALPHA, BlendingMode.ONE_MINUS_SRC_ALPHA);
        }
        await _context.EndBatchAsync();
#endif

#if true
        var _renderesCount = 0;
        foreach (var renderer in ComponentsBucket<Renderer>.Bucket)
        {
            if (!renderer.Entity.Enabled)
                continue;

            var shader = renderer.Material!.Shader;
            if (shader == null)
                continue;

            if (renderer.MeshType == MeshType.None)
                continue;

            var mesh = renderer.MeshType switch
            {
                MeshType.Quad => _quad,
                MeshType.Cube => _cube,
                MeshType.None => null,
                _ => null
            };

            _renderData[_renderesCount].Entity = renderer.Entity!;
            _renderData[_renderesCount].Mesh = mesh!;
            _renderData[_renderesCount].Shader = shader;
            _renderesCount++;
        }
#else
        var _renderesCount = 0;
        foreach (var entity in _currentScene.Root)
        {
            if (!entity.Enabled)
                continue;

            if (entity.TryGetComponents<Renderer>(out var renderers))
            {
                foreach (var renderer in renderers)
                {
                    var shader = renderer.Material.Shader;
                    if (shader == null)
                        continue;

                    if (renderer.MeshType == MeshType.None)
                        continue;

                    var mesh = renderer.MeshType switch
                    {
                        MeshType.Quad => _quad,
                        MeshType.Cube => _cube,
                        MeshType.None => null,
                        _ => null
                    };

                    _renderData[_renderesCount].Entity = entity;
                    _renderData[_renderesCount].Mesh = mesh;
                    _renderData[_renderesCount].Shader = shader;
                    _renderesCount++;
                }
            }

            // TODO: Make proper tree iteration
            foreach (var child in entity.Children)
            {
                if (!child.Enabled)
                    continue;

                if (child.TryGetComponents<Renderer>(out var childRenderers))
                {
                    foreach (var renderer in childRenderers)
                    {
                        var shader = renderer.Material.Shader;
                        if (shader == null)
                            continue;

                        if (renderer.MeshType == MeshType.None)
                            continue;

                        var mesh = renderer.MeshType switch
                        {
                            MeshType.Quad => _quad,
                            MeshType.Cube => _cube,
                            MeshType.None => null,
                        };

                        _renderData[_renderesCount].Entity = child;
                        _renderData[_renderesCount].Mesh = mesh;
                        _renderData[_renderesCount].Shader = shader;
                        _renderesCount++;
                    }
                }
            }
        }
#endif

#if false
        // TODO: Instancing
        Array.Sort(_renderData, 0, _renderesCount);
        Shader? _currentShader = default;
        Mesh? _currentMesh = default;

        await _context.BeginBatchAsync();
        for (int i = 0; i < _renderesCount; i++)
        {
            var (entity, mesh, shader) = _renderData[i];

            if (_currentShader != shader)
            {
                _currentShader = shader;
                await shader.Bind(_context);

                // Global uniforms.
                await _context.UniformAsync(shader.Time, Time.CurrentTime);
                await _context.UniformMatrixAsync(shader.WorldToView, false, _camera.WorldToView);
                await _context.UniformMatrixAsync(shader.Projection, false, _camera.Projection);
                var cameraPosition = _camera.Entity.Transform.Position;
                await _context.UniformAsync(
                    shader.CameraPositionWS,
                    cameraPosition.X,
                    cameraPosition.Y,
                    cameraPosition.Z
                );
            }

            // Object specific.
            await _context.UniformMatrixAsync(shader.ObjectToWorld, false, entity.Transform.Matrix);
            await _context.UniformMatrixAsync(shader.InvObjectToWorld, false, entity.Transform.InvMatrix);

            await mesh.Bind(_context, shader);
            await _context.DrawElementsAsync(
                Primitive.TRIANGLES,
                mesh.Indices.Length,
                DataType.UNSIGNED_SHORT,
                0
            );
            await mesh.UnBind(_context, shader);
        }

        await _context.EndBatchAsync();
#endif
    }

    public void Dispose()
    {
        _physicsWorld.Dispose();
        _currentScene.Dispose();
    }
}