using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public static class BlendStates
{
    public static readonly BlendState LightingBlend = new()
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        ColorSourceBlend = Blend.SourceColor,
        ColorDestinationBlend = Blend.One,
    };
    
    public static readonly BlendState MultiplyBlend = new()
    {
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.Zero,
    
        AlphaBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.DestinationAlpha,
        AlphaDestinationBlend = Blend.Zero
    };
}