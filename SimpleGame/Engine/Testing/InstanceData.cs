using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.Testing;

public struct InstanceData : IVertexType
{
    public Vector2 Position;
    public Vector2 Scale;
    public Color Color;
    public Vector4 UV;

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),
        new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        new VertexElement(20, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}