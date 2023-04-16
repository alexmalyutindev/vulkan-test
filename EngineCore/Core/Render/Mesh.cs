namespace MtgWeb.Core.Render;

public class Mesh
{
    public float[] Vertices;
    public float[] UVs;
    public float[] Normals;
    public UInt16[] Indices;

#if false
    private WebGLBuffer _verticesBuffer;
    private WebGLBuffer _uvsBuffer;
    private WebGLBuffer _indicesBuffer;

    public async Task Init(WebGLContext context)
    {
        _verticesBuffer = await context.CreateBufferAsync();
        await context.BindBufferAsync(BufferType.ARRAY_BUFFER, _verticesBuffer);
        await context.BufferDataAsync(BufferType.ARRAY_BUFFER, Vertices, BufferUsageHint.STATIC_DRAW);

        _uvsBuffer = await context.CreateBufferAsync();
        await context.BindBufferAsync(BufferType.ARRAY_BUFFER, _uvsBuffer);
        await context.BufferDataAsync(BufferType.ARRAY_BUFFER, UVs, BufferUsageHint.STATIC_DRAW);

        _indicesBuffer = await context.CreateBufferAsync();
        await context.BindBufferAsync(BufferType.ELEMENT_ARRAY_BUFFER, _indicesBuffer);
        await context.BufferDataAsync(BufferType.ELEMENT_ARRAY_BUFFER, Indices, BufferUsageHint.STATIC_DRAW);
    }

    public async Task Bind(WebGLContext context, Shader shader)
    {
        if (shader.PositionOS != -1)
        {
            var attributeId = (uint) shader.PositionOS;
            await context.BindBufferAsync(BufferType.ARRAY_BUFFER, _verticesBuffer);
            await context.VertexAttribPointerAsync(attributeId, 3, DataType.FLOAT, false, 0, 0);
            await context.EnableVertexAttribArrayAsync(attributeId);
        }

        if (shader.Texcoord != -1)
        {
            var attributeId = (uint) shader.Texcoord;
            await context.BindBufferAsync(BufferType.ARRAY_BUFFER, _uvsBuffer);
            await context.VertexAttribPointerAsync(attributeId, 2, DataType.FLOAT, false, 0, 0);
            await context.EnableVertexAttribArrayAsync(attributeId);
        }

        await context.BindBufferAsync(BufferType.ELEMENT_ARRAY_BUFFER, _indicesBuffer);
    }

    public async Task UnBind(WebGLContext context, Shader shader)
    {
        if (shader.PositionOS != -1)
        {
            await context.DisableVertexAttribArrayAsync((uint) shader.PositionOS);
        }

        if (shader.Texcoord != -1)
        {
            await context.DisableVertexAttribArrayAsync((uint) shader.Texcoord);
        }

        await context.BindBufferAsync(BufferType.ELEMENT_ARRAY_BUFFER, null);
    }
#endif

    private static Mesh? _quad;
    private static Mesh? _cube;

    public static Mesh Quad()
    {
        if (_quad != null)
            return _quad;

        _quad = new Mesh
        {
            Vertices = new[]
            {
                -0.5f, 0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                0.5f, 0.5f, 0.0f
            },
            UVs = new[]
            {
                0.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f
            },
            Indices = new UInt16[]
            {
                3, 2, 1,
                3, 1, 0
            }
        };

        return _quad;
    }

    public static Mesh Cube()
    {
        if (_cube != null)
            return _cube;

        _cube = new Mesh
        {
            Vertices = new float[]
            {
                // Front face
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, 0.5f,
                // Back face
                -0.5f, -0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                // Top face
                -0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                // Bottom face
                -0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, 0.5f,
                -0.5f, -0.5f, 0.5f,
                // Right face
                0.5f, -0.5f, -0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                // Left face
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, 0.5f,
                -0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, -0.5f,
            },
            UVs = new[]
            {
                // Front
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Back
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Top
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Bottom
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Right
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Left
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
            },
            Indices = new UInt16[]
            {
                0, 1, 2, 0, 2, 3, // front
                4, 5, 6, 4, 6, 7, // back
                8, 9, 10, 8, 10, 11, // top
                12, 13, 14, 12, 14, 15, // bottom
                16, 17, 18, 16, 18, 19, // right
                20, 21, 22, 20, 22, 23, // left
            }
        };

        return _cube;
    }
}