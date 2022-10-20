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
		float Speed1		: packoffset(c0.x); [Default(0.2)]
		float Speed2		: packoffset(c0.y); [Default(0.3)]
		float Brightness	: packoffset(c0.z); [Default(1.0)]
		float Constract 	: packoffset(c0.w); [Default(1.5)]		
		float3 WaterColor	: packoffset(c1.x);   [Default(0,0,1)]		
		float3 WaterHightly : packoffset(c2.x);   [Default(0.5,0.8,1)]		
	}

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 10_0]
	[Entrypoints VS=VS PS=PS]

	static const float PI = 3.14159265f;

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
		float amplitude = 0.1;
		float frequency = 5.0;
		float octaves = 6;
		float lacunarity = 2.0;
		float gain = 0.5;
		
		for (int i = 0; i < octaves; i++)
		{
			value += amplitude * noise(position.xz * frequency + Time * Speed1);
			frequency *= lacunarity;
			amplitude *= gain;
		}
		
		return value;
	}
	
	// The wave options
	struct gln_tGerstnerWaveOpts {
	    float2 direction;   // Direction of the wave
	    float steepness;  // Steepness/Sharpness of the peaks
	    float wavelength; // Wavelength...self explnitory
	};
	
	float3 gln_GerstnerWave(float3 p, gln_tGerstnerWaveOpts opts, float time) {
	    float steepness = opts.steepness;
	    float wavelength = opts.wavelength;
	
	    float k = 2.0 * PI / wavelength;
	    float c = sqrt(9.8 / k);
	    float2 d = normalize(opts.direction);
	    float f = k * (dot(d, p.xz) - c * time);
	    float a = steepness / k;
	
	    return float3(
	        d.x * (a * cos(f)),
	        a * sin(f),
	        d.y * (a * cos(f))
	    );
	}	

	static gln_tGerstnerWaveOpts A = { float2(0.0, -1.0), 0.5, 0.2 };
	static gln_tGerstnerWaveOpts B = {float2(0.0, 1.0), 0.25, 0.4};
	static gln_tGerstnerWaveOpts C = {float2(1.0, 1.0), 0.15, 0.6};
	static gln_tGerstnerWaveOpts D = {float2(1.0, 1.0), 0.4, 0.2};
	
	float3 Displace(float3 pos)
	{
		float3 p = pos; 
		float3 n = float3(0.0, 0.0, 0.0);
		
		n.y += fbm(pos);
		
		n += gln_GerstnerWave(p, A, Time * Speed2) * 0.8;
	    n += gln_GerstnerWave(p, B, Time * Speed2) * 0.5;
	    n += gln_GerstnerWave(p, C, Time * Speed2) * 0.25;
	    n += gln_GerstnerWave(p, D, Time * Speed2) * 0.1;
		
		return pos + n;
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
		
		float3 pos3 = Displace(input.Position);
		
		float4 pos4 = float4(pos3, 1.0);
		
		output.pos = mul(pos4, WorldViewProj);
		output.Nor = pos3;
		output.Tex = input.TexCoord;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		float mask = input.Nor.y * Constract;
		float3 diffuseColor = lerp(WaterColor, WaterHightly, mask);
		diffuseColor *= Brightness;
	
		return float4(diffuseColor, 1);
	}

[End_Pass]