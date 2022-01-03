[Begin_ResourceLayout]

	[Directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};
	
	cbuffer PerFrame : register(b1)
	{
		float Time				: packoffset(c0); [Time]
	}

	cbuffer Parameters : register(b2)
	{
		float4 TopColor			  : packoffset(c0);   [Default(0.0,1.0,0.0, 1.0)]
		float4 BottomColor		  : packoffset(c1);   [Default(1.0, 0.0, 0.0,1.0)]
		float BendRotationRandom  : packoffset(c2.x); [Default(0.3)]
		float BladeWidth		  : packoffset(c2.y); [Default(0.005)]
		float BladeWidthRandom    : packoffset(c2.z); [Default(0.001)]
		float BladeHeight		  : packoffset(c2.w); [Default(0.06)]
		float BladeHeightRandom   : packoffset(c3.x); [Default(0.02)]
		float2 WindFrenquency	  : packoffset(c3.y); [Default(0.05,0.05)]
		float WindStrength		  : packoffset(c3.w); [Default(1.0)]
		float2 WindTextureSize    : packoffset(c4.x); [Default(512, 512)]
		float2 WindTextureOffset  : packoffset(c4.z); [Default(0.0, 0.0)]		
		float BladeForward		  : packoffset(c5.x); [Default(0.03)]
		float BladeCurvature	  : packoffset(c5.y); [Default(2)]
	};
	
	Texture2D WindDistortion : register(t0);
	SamplerState WindSampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[Profile 10_0]
	[Entrypoints VS=VS GS=GS PS=PS]

	static const float PI = 3.14159265f;
	#define BLADE_SEGMENTS 3

	struct VS_IN
	{
		float4 Position : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};
	
	struct GS_IN
	{
		float4 position : SV_POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

	struct PS_IN
	{
		float4 position : SV_POSITION;
		float2 uv 		: TEXCOORD;
	};

	GS_IN VS(VS_IN input)
	{
		GS_IN output = (GS_IN)0;

		output.position = input.Position; 		
		output.normal = input.normal;
		output.tangent = input.tangent;

		return output;
	}
	
	PS_IN VertexOutput(float3 pos, float2 uv)
	{
		PS_IN o;
		o.position = mul(float4(pos,1), WorldViewProj);
		o.uv = uv;
		return o;
	}
	
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
	    float c, s;
	    sincos(angle, s, c);
	
	    float t = 1 - c;
	    float x = axis.x;
	    float y = axis.y;
	    float z = axis.z;
	
	    return float3x3(
	        t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
	        t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
	        t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
	    );
	}
	   
    float rand (float2 v)
    {
        return frac(sin(dot(v,float2(12.9898,78.233)))*43758.5453123);
    }
	
	PS_IN GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float2 uv, float3x3 transformMatrix)
	{
		float3 tangentPoint = float3(width, forward, height);
	
		float3 localPosition = vertexPosition + mul(transformMatrix, tangentPoint);
		return VertexOutput(localPosition, uv);
	}
	
	[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
	void GS(triangle GS_IN input[3] : SV_POSITION, inout TriangleStream<PS_IN> triStream)
	{
		float3 vNormal = input[0].normal;
		float4 vTangent = input[0].tangent;
		float3 vBinormal = cross(vNormal, vTangent.xyz) * vTangent.w;
		
		float3x3 tangentToLocal = float3x3(
			vTangent.x, vBinormal.x, vNormal.x,
			vTangent.y, vBinormal.y, vNormal.y,
			vTangent.z, vBinormal.z, vNormal.z
		);
		
		float4 pos = input[0].position;
		float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos.xy) * (PI*2), float3(0, 0, 1));
		float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zx) * BendRotationRandom * PI * 0.5, float3(-1, 0, 0));
		
		float2 uv  = pos.xz * WindTextureSize + WindTextureOffset + WindFrenquency * Time;
		float2 windSample = (WindDistortion.SampleLevel(WindSampler, uv, 0).xy * 2 -1) * WindStrength;
		float3 wind = normalize(float3(windSample.x, windSample.y, 0));
		float3x3 windRotation = AngleAxis3x3(PI * windSample, wind);
		
		float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, windRotation), facingRotationMatrix), bendRotationMatrix);
		float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);

		float height = (rand(pos.zx) * 2 - 1) * BladeHeightRandom + BladeHeight;
		float width = (rand(pos.xy) * 2 - 1) * BladeWidthRandom + BladeWidth;
				
		float forward = rand(pos.yz) * BladeForward;
		for (int i =0 ; i < BLADE_SEGMENTS; i++)
		{
			float t = i / (float)BLADE_SEGMENTS;
			float segmentHeight = height * t;
			float segmentWidth = width * (1-t);
			float segmentForward = pow(t, BladeCurvature) * forward;
			
			float3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;
			
			triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0,t), transformMatrix));		
			triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1,t), transformMatrix));						
		}
		
		triStream.Append(GenerateGrassVertex(pos, 0, height, forward, float2(0.5,1), transformationMatrix));
	}

	#if !GAMMA_COLORSPACE
	float4 GammaToLinear(const float4 color)
	{
		return float4(pow(color.rgb, 2.2), color.a);
	}
	#endif

	float4 PS(PS_IN input) : SV_Target
	{
		float4 color = lerp(BottomColor, TopColor, input.uv.y);
		return GammaToLinear(color);
	}

[End_Pass]