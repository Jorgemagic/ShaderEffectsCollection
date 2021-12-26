[Begin_ResourceLayout]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};

	cbuffer Parameters : register(b1)
	{
		float HeightAmount			: packoffset(c0);   [Default(1.0)]
	};
	
	Texture2D HeightMap : register(t0);
	SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 11_0]
	[Entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float3 Normal	: NORMAL;
		float2 TexCoord : TEXCOORD;
	};
	
	
	struct PS_IN
	{
		float4 Position : SV_POSITION;
		float3 Normal	: NORMAL;
		float2 TexCoord : TEXCOORD;
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		float height = HeightMap.SampleLevel(Sampler, input.TexCoord, 0).r;
		
		float4 pos = input.Position;
		pos.xyz += input.Normal * (height * HeightAmount);
		output.Position = mul(pos, WorldViewProj);
		output.Normal = input.Normal;
		output.TexCoord = input.TexCoord;

		return output;
	}
	

	float4 PS(PS_IN input) : SV_Target
	{
		return HeightMap.Sample(Sampler, input.TexCoord);
	}

[End_Pass]