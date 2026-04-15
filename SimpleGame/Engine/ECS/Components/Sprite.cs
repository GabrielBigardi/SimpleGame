using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.ECS.Components;

public struct Sprite
{
    public Texture2D Texture;
    public Vector2 Scale;
    public Vector2 Origin;
    public Vector2 HalfSize;
    public Rectangle Source;
    public Color Color;
}