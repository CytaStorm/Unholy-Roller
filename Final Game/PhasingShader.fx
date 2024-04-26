#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float amount;

struct VertexShaderOutput
{
	float4 Color : COLOR0;
    float2 texCoord : TEXCOORD0;
};

sampler2D texSampler;

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 col = tex2D(texSampler, input.texCoord);
	
    if (col.a > 0 && input.texCoord.y <= amount)
    {
		// Make pixel black
		col.rgb = 0;
    }
	
    return col;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};