using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class LightSource : Component
{
    public Transform Transform;
    public Sprite Sprite;

    public void Draw(SpriteBatch spriteBatch, float intensity = 1f)
    {
        spriteBatch.Draw(Sprite.Texture, Transform.Position, null, Sprite.ColorTint * intensity, Transform.Rotation, Sprite.CenterOrigin, Transform.Scale, SpriteEffects.None, 0f);
    }
}