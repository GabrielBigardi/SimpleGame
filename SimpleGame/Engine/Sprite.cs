using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Size =>  new Vector2(Texture.Width, Texture.Height);
    public Vector2 Position;
    public Vector2 Scale;
    public Vector2 CenterOrigin => new(Texture.Width / 2f, Texture.Height / 2f);
    public float Rotation;
    public Color ColorTint = Color.White;
    public bool FlipX;
    
    public Sprite(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f)
    {
        Texture = texture;
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position, null, ColorTint, Rotation, CenterOrigin, Scale, FlipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }
}