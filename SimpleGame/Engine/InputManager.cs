using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleGame.Engine;

public static class InputManager
{
    private static KeyboardState _currentKeyState, _previousKeyState;
    private static MouseState _currentMouseState, _previousMouseState;

    public static Vector2 RawPlayerInput => new(
        Convert.ToInt32(IsKeyDown(Keys.D)) - Convert.ToInt32(IsKeyDown(Keys.A)),
        Convert.ToInt32(IsKeyDown(Keys.S)) - Convert.ToInt32(IsKeyDown(Keys.W)));

    public static Vector2 NormalizedPlayerInput
    {
        get
        {
            var input = RawPlayerInput;

            if (input != Vector2.Zero)
                input.Normalize();

            return input;
        }
    }
    
    public static Point MousePosition => _currentMouseState.Position;
    public static Point PreviousMousePosition => _previousMouseState.Position;
    public static Vector2 MouseDelta => (_currentMouseState.Position - _previousMouseState.Position).ToVector2();
    public static int ScrollDelta => _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;


    public static void Update()
    {
        _previousKeyState = _currentKeyState;
        _currentKeyState = Keyboard.GetState();
        
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
    }

    public static bool KeyPressedThisFrame(params Keys[] keys)
    {
        foreach (Keys key in keys)
        {
            if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                return true;
        }

        return false;
    }

    public static bool KeyReleasedThisFrame(params Keys[] keys)
    {
        foreach (Keys key in keys)
        {
            if (_currentKeyState.IsKeyUp(key) && _previousKeyState.IsKeyDown(key))
                return true;
        }

        return false;
    }

    public static bool IsKeyDown(params Keys[] keys)
    {
        foreach (Keys key in keys)
        {
            if (_currentKeyState.IsKeyDown(key))
                return true;
        }

        return false;
    }
    
    public static bool IsLeftMouseDown() =>
        _currentMouseState.LeftButton == ButtonState.Pressed;

    public static bool IsRightMouseDown() =>
        _currentMouseState.RightButton == ButtonState.Pressed;

    public static bool IsMiddleMouseDown() =>
        _currentMouseState.MiddleButton == ButtonState.Pressed;

    public static bool LeftMousePressedThisFrame() =>
        _currentMouseState.LeftButton == ButtonState.Pressed &&
        _previousMouseState.LeftButton == ButtonState.Released;

    public static bool LeftMouseReleasedThisFrame() =>
        _currentMouseState.LeftButton == ButtonState.Released &&
        _previousMouseState.LeftButton == ButtonState.Pressed;

    public static bool RightMousePressedThisFrame() =>
        _currentMouseState.RightButton == ButtonState.Pressed &&
        _previousMouseState.RightButton == ButtonState.Released;

    public static bool RightMouseReleasedThisFrame() =>
        _currentMouseState.RightButton == ButtonState.Released &&
        _previousMouseState.RightButton == ButtonState.Pressed;

    public static bool MiddleMousePressedThisFrame() =>
        _currentMouseState.MiddleButton == ButtonState.Pressed &&
        _previousMouseState.MiddleButton == ButtonState.Released;

    public static bool MiddleMouseReleasedThisFrame() =>
        _currentMouseState.MiddleButton == ButtonState.Released &&
        _previousMouseState.MiddleButton == ButtonState.Pressed;
}