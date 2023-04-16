namespace MtgWeb.Core.Render;

public class Shader
{
    public readonly string Name;
    public int Queue;

    public bool IsLoaded { get; private set; }
    public bool IsCompiled { get; private set; }

    public int PositionOS { get; private set; }
    public int Texcoord { get; private set; }

    private String _vertexSrc;
    private String _fragmentSrc;

    private static readonly Dictionary<string, Shader> ShadersLibrary = new();

    private Shader(string name)
    {
        Name = name;
        ShadersLibrary.Add(Name, this);
    }

    public static Shader Create(string name)
    {
        if (ShadersLibrary.TryGetValue(name, out var shader))
        {
            return shader;
        }

        return new Shader(name);
    }

    public async Task Load()
    {
        var (vertex, fragment) = await Resources.LoadShader(Name);
        _vertexSrc = vertex;
        _fragmentSrc = fragment;

        IsLoaded = true;
    }
    
    public static async Task<Shader> Load(String name)
    {
        if (ShadersLibrary.TryGetValue(name, out var shader))
        {
            if (!shader.IsLoaded)
                shader.Load();
            return shader;
        }

        var (vertex, fragment) = await Resources.LoadShader(name);
        shader = new Shader(name)
        {
            _vertexSrc = vertex,
            _fragmentSrc = fragment,
            IsLoaded = true
        };

        return shader;
    }
}