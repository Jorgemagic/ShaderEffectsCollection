[Begin_ResourceLayout]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};
	
	cbuffer PerFrame : register(b1)
	{
		float Time 			: packoffset(c0); [Time]
	}

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 10_0]
	[Entrypoints VS=VS PS=PS]

	float random (in float2 st) {
	    return frac(sin(dot(st.xy,float2(12.9898,78.233)))* 43758.5453123);
	}

	// Based on Morgan McGuire @morgan3d
	// https://www.shadertoy.com/view/4dS3Wd
	float noise (in float2 st) {
	    float2 i = floor(st);
	    float2 f = frac(st);
	
	    // Four corners in 2D of a tile
	    float a = random(i);
	    float b = random(i + float2(1.0, 0.0));
	    float c = random(i + float2(0.0, 1.0));
	    float d = random(i + float2(1.0, 1.0));
	
	    float2 u = f * f * (3.0 - 2.0 * f);
	
	    return lerp(a, b, u.x) +
	            (c - a)* u.y * (1.0 - u.x) +
	            (d - b) * u.x * u.y;
	}
	
	float fbm(float3 position)
	{
		float value = 0;
		float amplitude = 0.2;
		float frequency = 2.0;
		float octaves = 6;
		float lacunarity = 2.0;
		float gain = 0.5;
		
		for (int i = 0; i < octaves; i++)
		{
			value += amplitude * noise(position.xz * frequency + Time);
			frequency *= lacunarity;
			amplitude *= gain;
		}
		
		return value;
	}


	struct VS_IN
	{
		float3 Position : POSITION;
		float3 Normal	: NORMAL;
		float2 TexCoord : TEXCOORD;
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float3 Nor	: NORMAL;
		float2 Tex : TEXCOORD;
	};	

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		float3 pos3 = input.Position + float3(0, fbm(input.Position),0);
		
		float4 pos4 = float4(pos3, 1.0);
		
		output.pos = mul(pos4, WorldViewProj);
		output.Nor = pos3;
		output.Tex = input.TexCoord;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		return float4(0,0,input.Nor.y, 1);
	}

[End_Pass]