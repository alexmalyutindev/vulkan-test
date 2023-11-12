using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace EngineCore;

public struct Attributes
{
    public Vector3D<float> PositionOS;
    public Vector4D<float> Color;
    public Vector4D<float> Texcoord0;

    public static VertexInputBindingDescription GetBindingDescription()
    {
        VertexInputBindingDescription bindingDescription = new()
        {
            Binding = 0,
            Stride = (uint) Unsafe.SizeOf<Attributes>(),
            InputRate = VertexInputRate.Vertex,
        };

        return bindingDescription;
    }

    private const Format Float2 = Format.R32G32Sfloat;
    private const Format Float3 = Format.R32G32B32Sfloat;
    private const Format Float4 = Format.R32G32B32A32Sfloat;

    public static VertexInputAttributeDescription[] GetAttributeDescriptions()
    {
        var attributeDescriptions = new[]
        {
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 0,
                Format = Float3,
                Offset = (uint) Marshal.OffsetOf<Attributes>(nameof(PositionOS)),
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 1,
                Format = Float4,
                Offset = (uint) Marshal.OffsetOf<Attributes>(nameof(Color)),
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 2,
                Format = Float4,
                Offset = (uint) Marshal.OffsetOf<Attributes>(nameof(Texcoord0)),
            }
        };

        return attributeDescriptions;
    }
}