using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Newtonsoft.Json;
using Silk.NET.Maths;

namespace MtgWeb.Core.Physics;

public class StaticBody
{
    public Vector3D<float> Offset;
    public StaticDescription Description;
    [JsonConverter(typeof(ShapeConverter))]

    public IShape Shape;
    public TypedIndex ShapeId;
}