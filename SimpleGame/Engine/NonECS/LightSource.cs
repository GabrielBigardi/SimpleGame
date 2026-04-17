using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.NonECS;

public class LightSource
{
    public Vector2 Position;
    public Vector2 Origin => new(AssetManager.LightGradientTexture .Width / 2f, AssetManager.LightGradientTexture.Height / 2f);
    public Color Color;
    public float Scale;

    public void Draw(SpriteBatch spriteBatch, float intensity = 1f, bool debugRect = false)
    {
        if (debugRect)
        {
            var bla = new Vector2(AssetManager.LightGradientTexture.Width, AssetManager.LightGradientTexture.Height) * Scale;
            spriteBatch.DrawRectangle(Position - bla / 2f, bla, Color, 4f);
        }
        
        spriteBatch.Draw(AssetManager.LightGradientTexture, Position, null, Color * intensity, 0f, Origin, Scale, SpriteEffects.None, 0f);
    }
}