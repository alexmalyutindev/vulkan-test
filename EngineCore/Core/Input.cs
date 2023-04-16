using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace MtgWeb.Core;

public enum ButtonState
{
    None,
    Down,
    Press,
    Up
}

public static class Input
{
    public static Vector2D<float> Axis => new(_axisX, _axisY);

    public static ButtonState Fire { get; private set; }

    public static Vector2 MouseDelta { get; private set; }
    public static Vector2 MousePosition { get; private set; }
    public static Vector2 PrevMousePosition { get; private set; }

    private static bool _firstUpdate = true;

    private static readonly ButtonState[] _keyMap = new ButtonState[350];

    private static float _axisX;
    private static float _axisY;

    private static Vector2 _rawMousePosition;
    private static bool _leftMouseButton;

    public static void Update()
    {
        if (_firstUpdate)
        {
            MousePosition = _rawMousePosition;
            PrevMousePosition = _rawMousePosition;
            _firstUpdate = false;
        }

        PrevMousePosition = MousePosition;
        MousePosition = _rawMousePosition;
        MouseDelta = MousePosition - PrevMousePosition;

        if (_leftMouseButton)
            Fire = Fire == ButtonState.None ? ButtonState.Down : ButtonState.Press;
        else
            Fire = Fire == ButtonState.Press ? ButtonState.Up : ButtonState.None;
    }

    public static void LateUpdate()
    {
        for (var i = 0; i < _keyMap.Length; i++)
        {
            switch (_keyMap[i])
            {
                case ButtonState.Down:
                    _keyMap[i] = ButtonState.Press;
                    break;
                case ButtonState.Up:
                    _keyMap[i] = ButtonState.None;
                    break;
            }
        }
    }

    public static ButtonState GetKeyState(Key keyCode)
    {
        return _keyMap[(int) keyCode];
    }

    private enum MouseButton
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 2,
        FORTH = 3,
        FIFTH = 4
    }

    // private const int LEFT_MOUSE = 0, RIGHT_MOUSE = 2;
    private static void OnMouseDown(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT:
                _leftMouseButton = true;
                break;
        }
    }

    private static void OnMouseUp(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT:
                _leftMouseButton = false;
                break;
        }
    }

    private static void OnMouseMove(Vector2 position, Vector2 delta)
    {
        _rawMousePosition += delta;
    }

    private static void OnKeyDown(int keyCode)
    {
        _keyMap[keyCode] = _keyMap[keyCode] != ButtonState.Press ? ButtonState.Down : _keyMap[keyCode];

        var key = (Key) keyCode;
        switch (key)
        {
            case Key.D:
                _axisX = 1;
                break;
            case Key.A:
                _axisX = -1;
                break;
            case Key.W:
                _axisY = 1;
                break;
            case Key.S:
                _axisY = -1;
                break;
        }
    }

    private static void OnKeyUp(int keyCode)
    {
        _keyMap[keyCode] = ButtonState.Up;

        var key = (Key) keyCode;
        switch (key)
        {
            case Key.D:
                _axisX = _axisX > 0 ? 0 : _axisX;
                break;
            case Key.A:
                _axisX = _axisX < 0 ? 0 : _axisX;
                break;
            case Key.W:
                _axisY = _axisY > 0 ? 0 : _axisY;
                break;
            case Key.S:
                _axisY = _axisY < 0 ? 0 : _axisY;
                break;
        }
    }
    
    public class Bridge : IDisposable
    {
        private IInputContext _context;

        public void Bind(IView view)
        {
            _context = view.CreateInput();
            _context.Keyboards[0].KeyDown += OnKeyDown;
            _context.Keyboards[0].KeyUp += OnKeyUp;
        }

        public void OnMouseDown(int button) => Input.OnMouseDown((MouseButton) button);

        public void OnMouseUp(int button) => Input.OnMouseUp((MouseButton) button);

        public void OnMouseMove(float x, float y, float dX, float dY)
        {
            Input.OnMouseMove(new Vector2(x, y), new Vector2(dX, dY));
        }

        public void OnKeyDown(IKeyboard keyboard, Key key, int arg3) => Input.OnKeyDown((int) key);
        public void OnKeyUp(IKeyboard keyboard, Key key, int arg3) => Input.OnKeyUp((int) key);
        public void Dispose()
        {
            _context.Keyboards[0].KeyDown -= OnKeyDown;
            _context.Keyboards[0].KeyUp -= OnKeyUp;
        }
    }
}