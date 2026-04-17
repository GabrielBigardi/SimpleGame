using Microsoft.Xna.Framework.Graphics;

namespace SimpleGame.Engine;

public static class BlendStates
{
    public static readonly BlendState LightCarveBlend = new()
    {
        
        // Multiply the white texture by 0. This forces the brush color to be pure Black.
        ColorSourceBlend = Blend.Zero, 
    
        // Multiply the destination by the inverse of the light's alpha.
        // Center of light (Alpha 1.0) -> Dest * 0.0 -> Turns the background pure Black.
        // Edge of light (Alpha 0.0) -> Dest * 1.0 -> Leaves the background completely untouched.
        ColorDestinationBlend = Blend.InverseSourceAlpha, 
    
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
    
        // Stick to normal Additive math to avoid the clamping black hole!
        ColorBlendFunction = BlendFunction.Add,
        AlphaBlendFunction = BlendFunction.Add
        
        //ColorSourceBlend = Blend.SourceAlpha,
        //ColorDestinationBlend = Blend.InverseSourceAlpha,
        //AlphaSourceBlend =  Blend.SourceAlpha,
        //AlphaDestinationBlend = Blend.InverseSourceAlpha,
        
        //ColorSourceBlend = Blend.SourceAlpha,
        //ColorDestinationBlend = Blend.InverseSourceAlpha,
        //AlphaSourceBlend =  Blend.SourceAlpha,
        //AlphaDestinationBlend = Blend.InverseSourceAlpha,
        //ColorBlendFunction = BlendFunction.ReverseSubtract,
        //AlphaBlendFunction =  BlendFunction.Add,
    };

    public static readonly BlendState LightingBlend = new()
    {
        ColorSourceBlend = Blend.SourceColor,
        ColorDestinationBlend = Blend.One,
        ColorBlendFunction = BlendFunction.ReverseSubtract
    };
}