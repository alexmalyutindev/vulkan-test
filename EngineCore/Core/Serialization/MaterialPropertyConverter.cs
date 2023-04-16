using MtgWeb.Core.Render;
using Newtonsoft.Json;

namespace MtgWeb.Core.Serialization;

public class MaterialPropertyConverter : JsonConverter<MaterialProperty>
{
    public override void WriteJson(JsonWriter writer, MaterialProperty? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value!.Value);
    }

    public override MaterialProperty? ReadJson(
        JsonReader reader,
        Type objectType,
        MaterialProperty? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var propertyValue = serializer.Deserialize(reader);

        var property = propertyValue switch
        {
            Int64 intValue => new MaterialProperty<int>
            {
                Value = (int) intValue,
                Type = MaterialPropertyType.Int
            },
            Double floatValue => new MaterialProperty<float>
            {
                Value = (float) floatValue,
                Type = MaterialPropertyType.Float
            },
            _ => new MaterialProperty {Type = MaterialPropertyType.Unknown}
        };

        return property;
    }
}