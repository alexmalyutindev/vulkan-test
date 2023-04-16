using MtgWeb.Core.Serialization;
using Newtonsoft.Json;

namespace MtgWeb.Core.Render;

public enum MaterialPropertyType
{
    Unknown = -1,
    Float = 0,
    Int = 1,
}

public class Material
{
    [JsonConverter(typeof(MaterialUniformsConverter))]
    public readonly Dictionary<string, MaterialProperty> Uniforms = new();

    [JsonConverter(typeof(ShaderConverter))]
    public Shader? Shader;

    // public async Task Init(WebGLContext context)
    // {
    //     if (Uniforms == null)
    //         return;
    //
    //     foreach (var property in Uniforms)
    //         await property.Value.Init(context, Shader);
    // }

    public async Task Load()
    {
        if (Shader != null)
            await Shader.Load();
    }
}

public class MaterialProperty
{
    public string Name;
    // public WebGLUniformLocation Location;
    public MaterialPropertyType Type;
    public virtual object Value { get; set; }
    // public virtual async Task Init(WebGLContext context, Shader shader) { }
}

public class MaterialProperty<T> : MaterialProperty
{
    public override object Value
    {
        get => _value;
        set => _value = (T) value;
    }

    public T _value;
    // public Func<WebGLContext, Task> Bind;
    //
    // public async Task Init(WebGLContext context, Shader shader)
    // {
    //     Location = await context.GetUniformLocationAsync(shader.Program, Name);
    //     Bind = async ctx =>
    //     {
    //         switch (Value)
    //         {
    //             case float value:
    //                 await ctx.UniformAsync(Location, value);
    //                 break;
    //             case int value:
    //                 await ctx.UniformAsync(Location, value);
    //                 break;
    //         }
    //     };
    // }
}