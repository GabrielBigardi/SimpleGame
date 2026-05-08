#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix ViewProjection;
Texture2D SpriteTexture;
sampler2D SpriteSampler = sampler_state { Texture = <SpriteTexture>; };

struct VertexShaderInput
{
    // Buffer 0 (The Base Quad)
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0; 
    
    // Buffer 1 (The Instance Data)
    float2 InstancePosition : POSITION1;
    float2 InstanceScale    : TEXCOORD1;
    float4 InstanceColor    : COLOR1;
    float4 InstanceUV       : TEXCOORD2; 
};

struct VertexToPixel
{
    // Note: Using SV_POSITION here so the OPENGL macro correctly maps it to POSITION
    float4 Position : SV_POSITION; 
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

VertexToPixel MainVS(in VertexShaderInput input)
{
    VertexToPixel output;

    // 1. Scale and Translate Position
    float4 worldPosition = input.Position;
    worldPosition.x *= input.InstanceScale.x;
    worldPosition.y *= input.InstanceScale.y;
    worldPosition.x += input.InstancePosition.x;
    worldPosition.y += input.InstancePosition.y;

    output.Position = mul(worldPosition, ViewProjection);
    output.Color = input.InstanceColor;
    
    // 2. Map the UVs to the Sprite Atlas
    float2 finalUV;
    finalUV.x = (input.TexCoord.x * input.InstanceUV.z) + input.InstanceUV.x;
    finalUV.y = (input.TexCoord.y * input.InstanceUV.w) + input.InstanceUV.y;
    
    output.TexCoord = finalUV;

    return output;
}

float4 MainPS(VertexToPixel input) : COLOR
{
    return tex2D(SpriteSampler, input.TexCoord) * input.Color;
}

technique Instancing
{
    pass Pass0
    {
        // Now using the dynamic macros defined at the top
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader  = compile PS_SHADERMODEL MainPS();
    }
}