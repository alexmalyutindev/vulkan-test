using MtgWeb.Core.Render;
using Newtonsoft.Json;

namespace MtgWeb.Core.Serialization;

public class MaterialUniformsConverter : JsonConverter<Dictionary<string, MaterialProperty>>
{
    public override void WriteJson(
        JsonWriter writer,
        Dictionary<string, MaterialProperty>? value,
        JsonSerializer serializer
    )
    {
        serializer.Serialize(
            writer,
            value!.ToDictionary(pair => pair.Key, pair => pair.Value.Value)
        );
    }

    public override Dictionary<string, MaterialProperty>? ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<string, MaterialProperty>? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonSerializer = new JsonSerializer()
        {
            Converters = {new MaterialPropertyConverter()}
        };
        var uniforms = jsonSerializer.Deserialize<Dictionary<string, MaterialProperty>>(reader);
        foreach (var property in uniforms)
        {
            property.Value.Name = property.Key;
        }

        return uniforms;
    }
}