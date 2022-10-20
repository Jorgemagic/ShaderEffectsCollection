[Begin_ResourceLayout]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};

	cbuffer PerFrame : register(b1)
	{
		float Time 			: packoffset(c0); [Time]
	}

	cbuffer Parameters : register(b2)
	{
		float3 Color			: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float4 WaveA			: packoffset(c1);   [Default(1,1,0.5,0.2)]		
		float4 WaveB			: packoffset(c2);	[Default(0,1,0.25,0.1)]
		float4 WaveC			: packoffset(c3);	[Default(1,1,0.15,0.05)]
		float Amplitude 		: packoffset(c4.x); [Default(0.1)];
		float Frequency			: packoffset(c4.y); [Default(5.0)];
		float Octaves 			: packoffset(c4.z); [Default(6)];
		float Lacunarity		: packoffset(c4.w); [Default(2.0)];
		float Gain				: packoffset(c5.x); [Default(0.5)];
		float Speed				: packoffset(c5.y); [Default(1.0)];
	};

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 10_0]
	[Entrypoints VS=VS PS=PS]


	static const float PI = 3.14159265f;
	
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
		float3 WorldPosition : POSITION1;
	};
	
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
		float f = Frequency;
		float a = Amplitude;
	
		for (int i = 0; i < Octaves; i++)
		{
			value += a * noise(position.xz * f + Time * Speed);
			f *= Lacunarity;
			a *= Gain;
		}
		
		return value;
	}
	
	float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
	{
		float steepness = wave.z;
		float wavelength = wave.w;
		float k = 2 * PI / wavelength;
		float c = sqrt(0.20 / k);
		float2 d = normalize(wave.xy);
		float f = k * (dot(d, p.xz) - c * Time);
		float a = steepness / k;
		
		tangent += float3(
						-d.x * d.x * (steepness * sin(f)),
						d.x * (steepness * cos(f)),
						-d.x * d.y * (steepness * sin(f))
						);
			
		binormal += float3(
						-d.x * d.y * (steepness * sin(f)),
						d.y * (steepness * cos(f)),
						-d.y * d.y * (steepness * sin(f))
						);
						
		return float3(
						d.x * (a * cos(f)),
						a * sin(f),
						d.y * (a * cos(f))
					  );
	}

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		float3 gridPoint = input.Position;
		float3 tangent = float3(1,0,0);
		float3 binormal = float3(0,0,1);
		float3 p = gridPoint;
		p.y += fbm(p);
		p += GerstnerWave(WaveA, gridPoint, tangent, binormal);
		p += GerstnerWave(WaveB, gridPoint, tangent, binormal);
		p += GerstnerWave(WaveC, gridPoint, tangent, binormal);
		float3 normal = normalize(cross(binormal, tangent));
		
		// Outputs
		output.pos = mul(float4(p,1), WorldViewProj);
		output.Nor = normal;
		output.Tex = input.TexCoord;
		output.WorldPosition = p;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		return float4(Color * saturate(input.WorldPosition.y * 0.3), 1);
		//return float4(input.Nor,1);
		//return float4(0,0,input.Nor.y,1);
	}

[End_Pass]