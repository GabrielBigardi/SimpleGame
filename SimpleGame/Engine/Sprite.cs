using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public class Sprite
{
    public Texture2D Texture;
    public Vector2 Position;
    public Vector2 Scale;
    public Vector2 CenterOrigin => new(Texture.Width / 2f, Texture.Height / 2f);
    public float Rotation;

    public Sprite(Texture2D texture, Vector2 position, Vector2 scale, float rotation = 0f)
    {
        Texture = texture;
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }
}