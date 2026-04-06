using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class Sprite : Component
{
    public Transform Transform;
    
    public Texture2D Texture;
    public Vector2 Size =>  new Vector2(Texture.Width, Texture.Height);
    public Vector2 CenterOrigin => new(Texture.Width / 2f, Texture.Height / 2f);

    public Color ColorTint = Color.White;
    public bool FlipX;

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Transform.Position, null, ColorTint, Transform.Rotation, CenterOrigin, Transform.Scale, FlipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }
}