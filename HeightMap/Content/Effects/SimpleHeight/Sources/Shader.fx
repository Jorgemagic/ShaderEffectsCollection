[Begin_ResourceLayout]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};

	cbuffer Parameters : register(b1)
	{
		float 	HeightAmount		: packoffset(c0.x); [Default(1.0)]
	};
	
	cbuffer Lights : register(b2)
	{
		float3 SunDirection : packoffset(c0.x); [SunDirection]
		float3 CameraPosition : packoffset(c1.x); [CameraPosition]
	}

Texture2D DiffuseTexture				: register(t0);
Texture2D HeightTexture 				: register(t1);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 11_0]
	[Entrypoints VS=VS PS=PS]

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

	float3 getNormal(float2 uv, float texelSize) 
	{
	    float u = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(0.0, -1.0), 0).r;
	    float r = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(-1.0, 0.0),0).r;
	    float l = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(1.0, 0.0),0).r;
	    float d = HeightTexture.SampleLevel(Sampler,  uv + texelSize * float2(0.0, 1.0),0).r;
	
	    float3 n;
	    n.z = u - d;
	    n.x = r - l;
	    n.y = 1.0 / 256;
	    return normalize(n);
	}

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;
		
		float height = HeightTexture.SampleLevel(Sampler, input.TexCoord,0).r;
		float3 pos = input.Position;
		pos += float3(0,1,0) * (height*HeightAmount);
		float4 position = float4(pos,1);

		output.pos = mul(position, WorldViewProj);
		
		float texelSize = 1.0/ 256;		
		float3 normal = getNormal(input.TexCoord, texelSize);	
		
		output.Nor = normal;
		output.Tex = input.TexCoord;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		//return DiffuseTexture.Sample(Sampler, input.Tex);
		
		//return float4(input.Nor, 1);
				
		//float height = HeightTexture.Sample(Sampler, input.Tex).r;
		//return float4(height.xxx,1);
		
		float3 normal = getNormal(input.Tex, 1.0/256);		
		return float4(normal, 1);	
	
	    // Note: Non-uniform scaling not supported
	    float diffuseLighting = saturate(dot(normal, -SunDirection)); // per pixel diffuse lighting
	
	    // Introduce fall-off of light intensity
	    diffuseLighting *= ((length(SunDirection) * length(SunDirection)));
	
	    // Using Blinn half angle modification for perofrmance over correctness
	    float3 h = normalize(normalize(CameraPosition.xyz - input.pos.xyz) - SunDirection);
	    float specLighting = pow(saturate(dot(h, normal)), 2.0f);
	
	    return saturate(float4(0.2,0.2,0.2,1.0) + (float4(1.0,1.0,1.0,1.0) * 0.9 * 0.6f) + (0.3 * 0.5f));
	}

[End_Pass]