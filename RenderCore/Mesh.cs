using System.Runtime.CompilerServices;
using Assimp;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace RenderCore;

public class Mesh : IDisposable
{
    // CPU
    private Attributes[] _vertices;
    private uint[] _indices;

    // GPU
    public Buffer VertexBuffer;
    public Buffer IndexBuffer;

    private DeviceMemory _vertexBufferMemory;
    private DeviceMemory _indexBufferMemory;
    public uint IndicesCount => (uint) _indices.Length;

    public static Mesh Load(string path)
    {
        using var context = new AssimpContext();
        var scene = context.ImportFile(path);
        var mesh = new Mesh();

        var vertexMap = new Dictionary<Attributes, uint>();
        var vertices = new List<Attributes>();
        var indices = new List<uint>();
        
        VisitSceneNode(scene.RootNode);
        
        mesh._vertices = vertices.ToArray();
        mesh._indices = indices.ToArray();

        void VisitSceneNode(Node node)
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

                        Attributes attributes = new Attributes
                        {
                            Pos = new Vector3D<float>(position.X, position.Y, position.Z),
                            Color = new Vector3D<float>(1, 1, 1),
                            //Flip Y for OBJ in Vulkan
                            TextCoord = new Vector2D<float>(texture.X, 1.0f - texture.Y)
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
                VisitSceneNode(node.Children[c]);
            }
        }

        return mesh;
    }

    public void LoadOnGPU()
    {
        VulkanContext.LoadBuffer(_vertices, ref VertexBuffer, ref _vertexBufferMemory);
        VulkanContext.LoadBuffer(_indices, ref IndexBuffer, ref _indexBufferMemory);
    }

    public void Dispose()
    {
        VulkanContext.DestroyBuffer(VertexBuffer);
        VulkanContext.FreeMemory(_vertexBufferMemory);
        
        VulkanContext.DestroyBuffer(IndexBuffer);
        VulkanContext.FreeMemory(_indexBufferMemory);
    }
}