using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SimpleGame.Engine;

public static class InputManager
{
    public static Vector2 RawInput;

    public static Vector2 NormalizedInput
    {
        get
        {
            if (RawInput == Vector2.Zero)
                return RawInput;

            var normalizedRawInput = RawInput;
            normalizedRawInput.Normalize();
            
            return normalizedRawInput;
        }
    }

    public static void Update()
    {
        //var horizontal = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A));
        //var vertical = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W));
        var horizontal = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.D)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.A));
        var vertical = Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.S)) - Convert.ToInt32(Keyboard.GetState().IsKeyDown(Keys.W));
        RawInput = new Vector2(horizontal, vertical);
    }
}