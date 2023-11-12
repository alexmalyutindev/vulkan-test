using Assimp;
using EngineCore.Rendering.Core;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace EngineCore.Rendering;

public class Mesh : IDisposable
{
    public uint IndicesCount => (uint) _indices.Length;

    // CPU
    private Attributes[] _vertices;
    private uint[] _indices;

    // GPU
    public Buffer VertexBuffer;
    public Buffer IndexBuffer;

    private DeviceMemory _vertexBufferMemory;
    private DeviceMemory _indexBufferMemory;

    public static Mesh Load(string path)
    {
        using var context = new AssimpContext();
        var scene = context.ImportFile(path);
        var mesh = new Mesh();

        var vertexMap = new Dictionary<Attributes, uint>();
        var vertices = new List<Attributes>();
        var indices = new List<uint>();

        VisitSceneNode(scene.RootNode, scene, vertexMap, indices, vertices);

        mesh._vertices = vertices.ToArray();
        mesh._indices = indices.ToArray();

        return mesh;
    }

    private static void VisitSceneNode(
        Node node,
        Scene scene,
        Dictionary<Attributes, uint> vertexMap,
        List<uint> indices,
        List<Attributes> vertices
    )
    {
        for (int m = 0; m < node.MeshCount; m++)
        {
            var mesh = scene.Meshes[node.MeshIndices[m]];

            for (int f = 0; f < mesh.FaceCount; f++)
            {
                var face = mesh.Faces[f];

                for (int i = 0; i < face.IndexCount; i++)
                {
                    int index = face.Indices[i];

                    var position = mesh.Vertices[index];
                    var texture = mesh.TextureCoordinateChannels[0][index];
                    var color = mesh.VertexColorChannels[0][index];

                    Attributes attributes = new Attributes
                    {
                        PositionOS = new Vector3D<float>(position.X, position.Y, position.Z),
                        Color = new Vector4D<float>(color.R, color.G, color.B, color.A),

                        //Flip Y for OBJ in Vulkan
                        Texcoord0 = new Vector4D<float>(texture.X, 1.0f - texture.Y, 0, 0)
                    };

                    if (vertexMap.TryGetValue(attributes, out var meshIndex))
                    {
                        indices.Add(meshIndex);
                    }
                    else
                    {
                        indices.Add((uint) vertices.Count);
                        vertexMap[attributes] = (uint) vertices.Count;
                        vertices.Add(attributes);
                    }
                }
            }
        }

        for (int c = 0; c < node.ChildCount; c++)
        {
            VisitSceneNode(node.Children[c], scene, vertexMap, indices, vertices);
        }
    }

    public void LoadOnGPU()
    {
        ExampleRenderer.LoadBuffer(_vertices, ref VertexBuffer, ref _vertexBufferMemory);
        ExampleRenderer.LoadBuffer(_indices, ref IndexBuffer, ref _indexBufferMemory);
    }

    public void Dispose()
    {
        ExampleRenderer.DestroyBuffer(VertexBuffer);
        ExampleRenderer.FreeMemory(_vertexBufferMemory);

        ExampleRenderer.DestroyBuffer(IndexBuffer);
        ExampleRenderer.FreeMemory(_indexBufferMemory);
    }
}