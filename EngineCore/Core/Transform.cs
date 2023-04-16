using System.Numerics;
using MtgWeb.Core.Utils;
using Newtonsoft.Json;
using Silk.NET.Maths;

namespace MtgWeb.Core;

public class Transform
{
    [JsonIgnore] public bool WasChanged { get; private set; } = false;

    public Vector3D<float> Position
    {
        get => _position;
        set
        {
            _isDirty = true;
            _position = value;
        }
    }

    public Vector3D<float> Rotation
    {
        get => _rotation;
        set
        {
            _isDirty = true;
            _rotation = value;
            _quaternion = Quaternion<float>.CreateFromYawPitchRoll(
                Scalar.DegreesToRadians(_rotation.Y),
                Scalar.DegreesToRadians(_rotation.X),
                Scalar.DegreesToRadians(_rotation.Z)
            );
        }
    }

    public Quaternion<float> Quaternion
    {
        get => _quaternion;
        set
        {
            _isDirty = true;
            _quaternion = value;
            // TODO: Quaternion to Euler
            // _rotation = _quaternion.ToEuler();
        }
    }

    public Vector3D<float> Scale
    {
        get => _scale;
        set
        {
            _isDirty = true;
            _scale = value;
        }
    }

    [JsonIgnore] public Vector3D<float> Right =>   new(Matrix.M11, Matrix.M21, Matrix.M31);
    [JsonIgnore] public Vector3D<float> Up =>      new(Matrix.M12, Matrix.M22, Matrix.M32);
    [JsonIgnore] public Vector3D<float> Forward => new(Matrix.M13, Matrix.M23, Matrix.M33);

    [JsonIgnore]
    public Matrix4X4<float> Matrix
    {
        get => _matrix;
        set
        {
            _isDirty = true;
            _matrix = value;
            Matrix4X4.Decompose(_matrix, out _scale, out _quaternion, out _position);
        }
    }

    public Matrix4X4<float> _matrix;

    [JsonIgnore]
    public Matrix4X4<float> InvMatrix;

    private bool _isDirty = true;

    private Vector3D<float> _position = Vector3D<float>.Zero;
    private Vector3D<float> _rotation = Vector3D<float>.Zero;
    private Quaternion<float> _quaternion = Quaternion<float>.Identity;
    private Vector3D<float> _scale = Vector3D<float>.One;

    public void Update()
    {
        if (WasChanged)
            WasChanged = false;

        if (!_isDirty)
            return;

        _isDirty = false;
        var translation = Matrix4X4.CreateTranslation(_position);
        var scale = Matrix4X4.CreateScale(_scale);
        var rotation = Matrix4X4.CreateFromQuaternion(_quaternion);

        Matrix = scale * rotation * translation;
        Matrix4X4.Invert(Matrix, out InvMatrix);

        WasChanged = true;
    }
}