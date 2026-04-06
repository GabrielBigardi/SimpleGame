using Microsoft.Xna.Framework;

namespace SimpleGame.Engine;

public class Transform : Component
{
    public Vector2 Position;
    public Vector2 Scale = Vector2.One;
    public float Rotation;
}