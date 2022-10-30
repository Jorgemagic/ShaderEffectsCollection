[Begin_ResourceLayout]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};
	
	cbuffer PerFrame : register(b1)
	{
		float Time				: packoffset(c0.x); [Time]
	}

	cbuffer Parameters : register(b2)
	{
		float Weight			: packoffset(c0.x);   [Default(0.4)]
	};
	
	Texture2D tExplosion    : register(t0);
	SamplerState Sampler    : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 11_0]
	[Entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float3 Normal 	: NORMAL;
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float Noise : TEXCOORD0;		
	};	
	
	// GLSL mod is not the same that fmod in HLSL so requires a specify implementation
	float3 mod(float3 x, float3 y) { return x - y * floor(x / y); }
	float3 mod289(float3 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
	float4 mod289(float4 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
	float4 permute(float4 x){return mod289(((x * 34.0) + 1.0) * x);}
	float4 taylorInvSqrt(float4 r) {   return 1.79284291400159 - 0.85373472095314 * r; }
	float3 fade(float3 t) {   return t*t*t*(t*(t*6.0-15.0)+10.0); }
	
	// Perlin Noise Stefan Gustavson
	float pnoise(float3 P, float3 rep)
	{
	    float3 Pi0 = mod(floor(P), rep);
	    float3 Pi1 = mod(Pi0 + 1.0f.xxx, rep);	    
	    Pi0 = mod289(Pi0);	    
	    Pi1 = mod289(Pi1);
	    float3 Pf0 = frac(P);
	    float3 Pf1 = Pf0 - 1.0f.xxx;
	    float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
	    float4 iy = float4(Pi0.yy, Pi1.yy);
	    float4 iz0 = Pi0.zzzz;
	    float4 iz1 = Pi1.zzzz;	    
	    
	    float4 ixy = permute(permute(ix) + iy);	    
	    float4 ixy0 = permute(ixy + iz0);	    
	    float4 ixy1 = permute(ixy + iz1);
	    
	    float4 gx0 = ixy0 * (1.0 / 7.0);
	    float4 gy0 = frac(floor(gx0) * (1.0 / 7.0)) - 0.5f.xxxx;
	    gx0 = frac(gx0);
	    float4 gz0 = (0.5f.xxxx - abs(gx0)) - abs(gy0);
	    float4 sz0 = step(gz0, 0.0f.xxxx);
	    gx0 -= (sz0 * (step(0.0f.xxxx, gx0) - 0.5f.xxxx));
	    gy0 -= (sz0 * (step(0.0f.xxxx, gy0) - 0.5f.xxxx));
	    
	    float4 gx1 = ixy1 * (1.0 / 7.0);
	    float4 gy1 = frac(floor(gx1) * (1.0 / 7.0)) - 0.5f.xxxx;
	    gx1 = frac(gx1);
	    float4 gz1 = (0.5f.xxxx - abs(gx1)) - abs(gy1);
	    float4 sz1 = step(gz1, 0.0f.xxxx);
	    gx1 -= (sz1 * (step(0.0f.xxxx, gx1) - 0.5f.xxxx));
	    gy1 -= (sz1 * (step(0.0f.xxxx, gy1) - 0.5f.xxxx));
	    
	    float3 g000 = float3(gx0.x, gy0.x, gz0.x);
	    float3 g100 = float3(gx0.y, gy0.y, gz0.y);
	    float3 g010 = float3(gx0.z, gy0.z, gz0.z);
	    float3 g110 = float3(gx0.w, gy0.w, gz0.w);
	    float3 g001 = float3(gx1.x, gy1.x, gz1.x);
	    float3 g101 = float3(gx1.y, gy1.y, gz1.y);
	    float3 g011 = float3(gx1.z, gy1.z, gz1.z);
	    float3 g111 = float3(gx1.w, gy1.w, gz1.w);
	    
	    float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
	    g000 *= norm0.x;
	    g010 *= norm0.y;
	    g100 *= norm0.z;
	    g110 *= norm0.w;
	    float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
	    g001 *= norm1.x;
	    g011 *= norm1.y;
	    g101 *= norm1.z;
	    g111 *= norm1.w;
	    
	    float n000 = dot(g000, Pf0);
	    float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
	    float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
	    float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
	    float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
	    float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
	    float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
	    float n111 = dot(g111, Pf1);
	    	    
	    float3 fade_xyz = fade(Pf0);
	    float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z.xxxx);
	    float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y.xx);
	    float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
	    return 2.2 * n_xyz;
	}
	
	float turbulence(float3 p)
	{
	    float w = 100.0f;
	    float t = -0.5f;
	    for (float f = 1.0f; f <= 10.0f; f += 1.0f)
	    {
	        float power = pow(2.0f, f);
	        t += abs(pnoise(float3(p * power), 10.0f.xxx) / power);
	    }
	    return t;
	}

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		float noise = turbulence( .5 * input.Normal + Time * 0.2 );

	    float displacement = - Weight * ( 10.0 *  -.10 * noise );
	    displacement += 5.0 * pnoise( 0.05 * input.Position.xyz ,
	    							  float3( 100.0.xxx ) );
	
	    output.Noise = noise;
	    float3 newPosition = input.Position.xyz + input.Normal * float3( displacement.xxx );
	    output.pos = mul(float4(newPosition, 1.0 ), WorldViewProj);		    

		return output;
	}
	
	static const float PI = 3.14159265358979323846264f;	
	
	float random( float4 position, float3 scale, float seed ){
	  return frac( sin( dot( position.xyz + seed, scale ) ) * 43758.5453 + seed ) ;
	}

	float4 PS(PS_IN input) : SV_Target
	{		
  		float r = .01 * random(input.pos, float3( 12.9898, 78.233, 151.7182 ), 0.0 );
  		
		float v = (1.1 * input.Noise + 1.) / 1.1;		
		float3 color = tExplosion.Sample(Sampler, float2(.5, v + r)).rgb;		
		return float4(color.xyz,1);		
	}

[End_Pass]