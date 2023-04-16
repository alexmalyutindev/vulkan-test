using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MtgWeb.Core.Serialization;

public class EntityConverter : JsonConverter<Entity>
{
    public override void WriteJson(JsonWriter writer, Entity? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override Entity? ReadJson(
        JsonReader reader,
        Type objectType,
        Entity? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jObject = serializer.Deserialize<JObject>(reader);
        if (jObject.TryGetValue("PrefabName", StringComparison.Ordinal, out _))
        {
            Console.WriteLine("Prefab!");
            return jObject.ToObject<PrefabEntity>();
        }
        else
        {
            return jObject.ToObject<Entity>();
        }
    }
}