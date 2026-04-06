using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class LightSource : Component
{
    public Transform Transform;
    public Sprite Sprite;
    public float Intensity = 1f;

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Sprite.Texture, Transform.Position, null, Sprite.ColorTint * Intensity, Transform.Rotation, Sprite.CenterOrigin, Transform.Scale, SpriteEffects.None, 0f);
    }
}