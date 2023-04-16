using MtgWeb.Core;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace EngineCore;

internal class FreeCamera : Component
{
    private Transform _cameraTransform;

    public void Inject(Camera camera)
    {
        _cameraTransform = camera.Entity.Transform;
    }

    public override void Update()
    {
        var axis = Input.Axis;
        if (axis.X != 0 || axis.Y != 0)
        {
            _cameraTransform.Position += (_cameraTransform.Forward * axis.Y - _cameraTransform.Right * axis.X) * 0.1f;
        }

        if (Input.GetKeyState(Key.Space) == ButtonState.Press)
        {
            _cameraTransform.Position -= _cameraTransform.Up * 0.1f;
        }
        
        if (Input.GetKeyState(Key.ShiftLeft) == ButtonState.Press)
        {
            _cameraTransform.Position += _cameraTransform.Up * 0.1f;
        }
    }
}