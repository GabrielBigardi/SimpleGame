using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine.Rendering;

public struct InstanceData : IVertexType
{
    public Vector2 Position;
    public Vector2 Scale;
    public Color Color;

    // This tells the GPU exactly how many bytes each variable takes up.
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 1),      // Position (Index 1 to avoid colliding with vertex geometry)
        new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1), // Scale
        new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 1)           // Color
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    
    public InstanceData(Vector2 position, Vector2 scale, Color color)
    {
        Position = position;
        Scale = scale;
        Color = color;
    }
}