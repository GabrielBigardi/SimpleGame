using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public static class BlendStates
{
    public static readonly BlendState LightCarveBlend = new()
    {
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaSourceBlend =  Blend.SourceAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaBlendFunction =  BlendFunction.Add,
    };

    public static readonly BlendState LightingBlend = new()
    {
        ColorSourceBlend = Blend.SourceColor,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend =  Blend.One,
        AlphaDestinationBlend = Blend.Zero,
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaBlendFunction =  BlendFunction.Add,
    };
}