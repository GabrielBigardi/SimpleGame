#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix Projection; // The 2D camera/screen projection
Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

// Input from the CPU Geometry (The Quad)
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

// Input from your InstanceData array
struct InstanceInput
{
	float2 InstancePosition : POSITION1;
	float2 InstanceScale : TEXCOORD1;
	float4 InstanceColor : COLOR1;
};

// Output sent to the Pixel Shader
struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input, in InstanceInput instance)
{
	VertexShaderOutput output;

	// Scale the quad, then move it to the instance's world position
	float2 worldPosition = (input.Position.xy * instance.InstanceScale) + instance.InstancePosition;
	
	// Project the 2D world position to the screen
	output.Position = mul(float4(worldPosition, 0, 1), Projection);
	
	// Pass color and UVs to the pixel shader
	output.Color = instance.InstanceColor;
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// Sample the texture and multiply by our instance color
	return tex2D(SpriteTextureSampler, input.TexCoord) * input.Color;
}

technique Instancing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}