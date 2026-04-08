using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.Utils;

public static class FontUtils
{
    public static Vector2 CalculateFontOriginTopLeft(string message, SpriteFont font)
    {
        return Vector2.Zero;
    }

    public static Vector2 CalculateFontOriginTopCenter(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X / 2f, 0f);
    }

    public static Vector2 CalculateFontOriginTopRight(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X, 0f);
    }

    public static Vector2 CalculateFontOriginMiddleLeft(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(0f, size.Y / 2f);
    }

    public static Vector2 CalculateFontOriginMiddleCenter(string message, SpriteFont font)
    {
        return font.MeasureString(message) / 2f;
    }

    public static Vector2 CalculateFontOriginMiddleRight(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X, size.Y / 2f);
    }

    public static Vector2 CalculateFontOriginBottomLeft(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(0f, size.Y);
    }

    public static Vector2 CalculateFontOriginBottomCenter(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X / 2f, size.Y);
    }

    public static Vector2 CalculateFontOriginBottomRight(string message, SpriteFont font)
    {
        return font.MeasureString(message);
    }
}