using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.ECS.Components;

public struct Shadow
{
    public Texture2D Texture;
    public Vector2 Offset;
    //public Vector2 Origin;
    public Rectangle Source;
    public Color Color;
    public float Scale;

}