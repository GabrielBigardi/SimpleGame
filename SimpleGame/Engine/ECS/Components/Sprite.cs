using Arch.AOT.SourceGenerator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.ECS.Components;

//[Component]
public struct Sprite
{
    public Texture2D Texture;
    public Vector2 Scale;
    public Vector2 Origin;
    public Vector2 HalfSize;
    public Color Color;
}