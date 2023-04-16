using MtgWeb.Core.Render;
using Newtonsoft.Json;

namespace MtgWeb.Core.Serialization;

public class ShaderConverter : JsonConverter<Shader>
{
    public override void WriteJson(JsonWriter writer, Shader? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, new ShaderData()
        {
            Name = value.Name,
            Queue = value.Queue
        });
    }

    private struct ShaderData
    {
        public string Name;
        public int Queue;
    }
    
    public override Shader? ReadJson(
        JsonReader reader,
        Type objectType,
        Shader? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var data = serializer.Deserialize<ShaderData>(reader);
        var shader = Shader.Create(data.Name);
        shader.Queue = data.Queue;

        return shader;
    }
}