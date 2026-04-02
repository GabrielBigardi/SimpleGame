using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class LightSource
{
    public Sprite Sprite;

    public void Draw(SpriteBatch spriteBatch, float intensityMultiplier = 1f)
    {
        spriteBatch.Draw(Sprite.Texture, Sprite.Position, null, Sprite.ColorTint * intensityMultiplier, Sprite.Rotation, Sprite.CenterOrigin, Sprite.Scale, SpriteEffects.None, 0f);
    }
}