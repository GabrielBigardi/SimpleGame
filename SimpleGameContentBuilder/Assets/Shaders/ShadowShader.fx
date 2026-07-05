#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

matrix LightView;
matrix LightProjection;

float3 LightDirection = float3(-1, -1, -1);
float4 DiffuseColor = float4(1, 1, 1, 1);
float4 AmbientColor = float4(0.5, 0.5, 0.5, 1);

Texture2D DiffuseTexture;
sampler2D DiffuseSampler = sampler_state
{
    Texture = <DiffuseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

Texture2D ShadowMap;
sampler2D ShadowSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

// =====================================
// SHADOW MAP GENERATION
// =====================================

struct ShadowVertexInput
{
    float4 Position : POSITION0;
};

struct ShadowVertexOutput
{
    float4 Position : SV_POSITION;
    float2 Depth : TEXCOORD0;
};

ShadowVertexOutput CreateShadowMapVS(ShadowVertexInput input)
{
    ShadowVertexOutput output;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, LightView);
    output.Position = mul(viewPosition, LightProjection);
    
    output.Depth = output.Position.zw;
    
    return output;
}

float4 CreateShadowMapPS(ShadowVertexOutput input) : COLOR0
{
    float depth = input.Depth.x / input.Depth.y;
    return float4(depth, 0, 0, 1);
}

// =====================================
// MAIN DRAWING WITH SHADOWS
// =====================================

struct MainVertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct MainVertexOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 LightSpacePos : TEXCOORD3;
};

MainVertexOutput DrawWithShadowMapVS(MainVertexInput input)
{
    MainVertexOutput output;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Pass normal (assuming uniform scale for simplicity)
    output.Normal = mul(input.Normal, (float3x3)World);
    output.Normal = normalize(output.Normal);
    
    output.TexCoord = input.TexCoord;
    
    // Calculate light space position for shadow mapping
    float4 lightViewPos = mul(worldPosition, LightView);
    output.LightSpacePos = mul(lightViewPos, LightProjection);
    
    return output;
}

float4 DrawWithShadowMapPS(MainVertexOutput input) : COLOR0
{
    // Directional light calculation
    float3 lightDir = normalize(-LightDirection);
    float NdotL = max(0, dot(input.Normal, lightDir));
    
    // Shadow Calculation
    float shadow = 1.0;
    
    // Convert to NDC and then to texture space
    float2 projectTexCoord;
    projectTexCoord.x = input.LightSpacePos.x / input.LightSpacePos.w / 2.0f + 0.5f;
    projectTexCoord.y = -input.LightSpacePos.y / input.LightSpacePos.w / 2.0f + 0.5f;
    
    if (saturate(projectTexCoord.x) == projectTexCoord.x && saturate(projectTexCoord.y) == projectTexCoord.y)
    {
        float currentDepth = input.LightSpacePos.z / input.LightSpacePos.w;
        // Bias to prevent shadow acne
        float bias = 0.005f; 
        
        float shadowMapDepth = tex2D(ShadowSampler, projectTexCoord).r;
        
        if (currentDepth - bias > shadowMapDepth)
        {
            shadow = 0.0; // In shadow
        }
    }
    
    float4 texColor = tex2D(DiffuseSampler, input.TexCoord);
    
    // If there is no texture bound, we assume white
    // In HLSL tex2D returns (0,0,0,0) if no texture is bound, which can be an issue if the model has no texture.
    // For simplicity, we just assume the model has a texture or we set a white 1x1 texture in code.
    
    float3 finalColor = texColor.rgb * (AmbientColor.rgb + (DiffuseColor.rgb * NdotL * shadow));
    
    return float4(finalColor, texColor.a);
}

technique CreateShadowMap
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL CreateShadowMapVS();
        PixelShader = compile PS_SHADERMODEL CreateShadowMapPS();
    }
}

technique DrawWithShadowMap
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL DrawWithShadowMapVS();
        PixelShader = compile PS_SHADERMODEL DrawWithShadowMapPS();
    }
}
