using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimpleGame.Engine.Managers;

namespace SimpleGame.Engine.NonECS;

public class LightSource
{
    public Vector2 Position;
    private Vector2 Origin => new(AssetManager.LightGradientTexture .Width * 0.5f, AssetManager.LightGradientTexture.Height * 0.5f);
    public Color Color;
    public float Scale;

    public void Draw(SpriteBatch spriteBatch, float intensity = 1f, bool debugRect = false)
    {
        if (debugRect)
        {
            var scale = new Vector2(AssetManager.LightGradientTexture.Width, AssetManager.LightGradientTexture.Height) * Scale;
            spriteBatch.DrawRectangle(Position - scale * 0.5f, scale, Color, 4f);
        }
        
        spriteBatch.Draw(AssetManager.LightGradientTexture, Position, null, Color * intensity, 0f, Origin, Scale, SpriteEffects.None, 0f);
    }
}