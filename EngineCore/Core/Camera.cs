using System.Numerics;
using Silk.NET.Maths;

namespace MtgWeb.Core;

public class Camera : Component
{
    public Matrix4X4<float> View;
    public Matrix4X4<float> Projection;

    public Matrix4X4<float> InvView;
    public Matrix4X4<float> ViewProjection;
    public Matrix4X4<float> InvViewProjection;

    public Vector4 ClearColor = new(0.8f, 0.8f, 0.8f, 1);

    public float AspectRatio = 16f / 9f;
    public float NearPlane = 0.01f;
    public float FarPlane = 100f;
    public float FieldOfView { get; set; } = 60f;

    public Camera()
    {
        Projection = Matrix4X4.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(45.0f),
            AspectRatio,
            NearPlane,
            FarPlane
        );
    }

    public override void Start()
    {
        UpdateMatrix(true);
    }

    public override void Update()
    {
        UpdateMatrix();
    }

    private void UpdateMatrix(bool forced = false)
    {
        var transform = Entity!.Transform;
        if (transform.WasChanged || forced)
        {
            // TODO: Fix view matrix
            var worldToView = Matrix4X4<float>.Identity * Matrix4X4.CreateTranslation(transform.Position);
            View = Matrix4X4.Transform(worldToView, transform.Quaternion);
            Matrix4X4.Invert(View, out InvView);

            ViewProjection = worldToView * Projection;
            Matrix4X4.Invert(ViewProjection, out InvViewProjection);
        }
    }
}