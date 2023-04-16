using BepuPhysics.Collidables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MtgWeb.Core.Physics;

public class ShapeConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override bool CanConvert(Type objectType) => objectType == typeof(IShape);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var shapeTypeId = jsonObject["Id"].Value<int>();

        IShape shape = shapeTypeId switch
        {
            Sphere.Id => new Sphere(),
            Capsule.Id => new Capsule(),
            Box.Id => new Box(),
            // TODO: Support other types.
            _ => throw new Exception($"Not supported type of shape: {shapeTypeId}!")
        };
        
        serializer.Populate(jsonObject.CreateReader(), shape);
        return shape;
    }
}