#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 OutlineColor = float4(0, 0, 0, 1); 
float2 TexelSize;                         
// CHANGED: Use a float instead of a bool to prevent the MojoShader swizzle error
float IncludeCorners = 1.0f;              

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    if (texColor.a < 0.01f) 
    {
        float alphaSum = tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(0, -TexelSize.y)).a +
                         tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(0, TexelSize.y)).a +
                         tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(-TexelSize.x, 0)).a +
                         tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(TexelSize.x, 0)).a;

        // CHANGED: Check the float value instead of a bool
        if (IncludeCorners > 0.5f)
        {
            alphaSum += tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(-TexelSize.x, -TexelSize.y)).a +
                        tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(TexelSize.x, -TexelSize.y)).a +
                        tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(-TexelSize.x, TexelSize.y)).a +
                        tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(TexelSize.x, TexelSize.y)).a;
        }

        if (alphaSum > 0.0f)
        {
            return OutlineColor;
        }
    }

    return texColor * input.Color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};