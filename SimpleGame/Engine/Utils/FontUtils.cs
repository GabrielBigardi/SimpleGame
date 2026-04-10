using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.Utils;

[Flags]
public enum OutlineFlags
{
    TopLeft = 1,
    TopCenter = 2,
    TopRight = 4,
    MiddleLeft = 8,
    MiddleRight = 16,
    BottomLeft = 32,
    BottomCenter = 64,
    BottomRight = 128,
    
    // Presets
    All = TopLeft | TopCenter | TopRight |
          MiddleLeft | MiddleRight |
          BottomLeft | BottomCenter | BottomRight,
    
    Cross = TopCenter | MiddleLeft | MiddleRight | BottomCenter,
}

public static class FontUtils
{
    #region Origin Utils
    public static Vector2 CalculateFontOriginTopLeft(string message, SpriteFont font)
    {
        return Vector2.Zero;
    }

    public static Vector2 CalculateFontOriginTopCenter(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X * 0.5f, 0f);
    }

    public static Vector2 CalculateFontOriginTopRight(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X, 0f);
    }

    public static Vector2 CalculateFontOriginMiddleLeft(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(0f, size.Y * 0.5f);
    }

    public static Vector2 CalculateFontOriginMiddleCenter(string message, SpriteFont font)
    {
        return font.MeasureString(message) * 0.5f;
    }

    public static Vector2 CalculateFontOriginMiddleRight(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X, size.Y * 0.5f);
    }

    public static Vector2 CalculateFontOriginBottomLeft(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(0f, size.Y);
    }

    public static Vector2 CalculateFontOriginBottomCenter(string message, SpriteFont font)
    {
        var size = font.MeasureString(message);
        return new Vector2(size.X * 0.5f, size.Y);
    }

    public static Vector2 CalculateFontOriginBottomRight(string message, SpriteFont font)
    {
        return font.MeasureString(message);
    }
    #endregion

    #region Outline String Extension Methods
    public static void DrawOutlinedString(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color textColor, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, OutlineFlags flags, Color outlineColor)
    {
        // Top part
        if ((flags & OutlineFlags.TopLeft) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(-scale, -scale), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        if ((flags & OutlineFlags.TopCenter) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(0f, -scale), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        if ((flags & OutlineFlags.TopRight) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(scale, -scale), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        // Middle part
        if ((flags & OutlineFlags.MiddleLeft) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(-scale, 0f), outlineColor, rotation, origin, scale, effects, layerDepth);

        if ((flags & OutlineFlags.MiddleRight) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(scale, 0f), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        // Bottom part
        if ((flags & OutlineFlags.BottomLeft) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(-scale, scale), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        if ((flags & OutlineFlags.BottomCenter) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(0f, scale), outlineColor, rotation, origin, scale, effects, layerDepth);
        
        if ((flags & OutlineFlags.BottomRight) != 0)
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(scale, scale), outlineColor, rotation, origin, scale, effects, layerDepth);

        
        spriteBatch.DrawString(spriteFont, text, position, textColor, rotation, origin, scale, effects, layerDepth);
    }
    #endregion
}