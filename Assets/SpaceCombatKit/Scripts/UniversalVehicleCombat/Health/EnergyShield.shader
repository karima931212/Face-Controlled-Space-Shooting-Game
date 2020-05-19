// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VSX/Energy Shield" {
    Properties 
	{

        _Color ("Tint (RGBA)", Color) = (1,1,1,1)

		// Buffer of hit positions, can add more
		_LocalHitPoint0 ("_LocalHitPoint0",Vector) = (0,0,0,1)
        _LocalHitPoint1 ("_LocalHitPoint1",Vector) = (0,0,0,0)
		_LocalHitPoint2 ("_LocalHitPoint2",Vector) = (0,0,0,0)
		_LocalHitPoint3 ("_LocalHitPoint3",Vector) = (0,0,0,0)
		_LocalHitPoint4 ("_LocalHitPoint4",Vector) = (0,0,0,0)
		_LocalHitPoint5 ("_LocalHitPoint5",Vector) = (0,0,0,0)
		_LocalHitPoint6 ("_LocalHitPoint6",Vector) = (0,0,0,0)
		_LocalHitPoint7 ("_LocalHitPoint7",Vector) = (0,0,0,0)
		_LocalHitPoint8 ("_LocalHitPoint8",Vector) = (0,0,0,0)
		_LocalHitPoint9 ("_LocalHitPoint9",Vector) = (0,0,0,0)
		
		_MeshScale ("Mesh Scale", float) = 1.0
        _SpreadFactor ("Spread Factor", float) = 1.0
        _MainTex ("Texture (RGB)", 2D) = "white" {}
    }
    SubShader {

        ZWrite Off
        Tags { "Queue" = "Transparent" }
        Blend One One
        Cull Off

        Pass { 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_fog_exp2

            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD2;
                float effectStrength:COLOR0;
            };

            uniform float _SpreadFactor;
			uniform float _MeshScale;

			uniform float4 _LocalHitPoint0;
            uniform float4 _LocalHitPoint1;
			uniform float4 _LocalHitPoint2;
			uniform float4 _LocalHitPoint3;
			uniform float4 _LocalHitPoint4;
			uniform float4 _LocalHitPoint5;
			uniform float4 _LocalHitPoint6;
			uniform float4 _LocalHitPoint7;
			uniform float4 _LocalHitPoint8;
			uniform float4 _LocalHitPoint9;
			
            uniform float4 _Color;
            sampler2D _MainTex;
			float4 _MainTex_ST;
            
			// Get the glow effect as an exponential function of distance from impact point
			float GetEffectValue(float strength, float _dist)
			{
				return (strength*(1/(1+pow(3, 5*(_dist-0.5)))));
			} 

            v2f vert (appdata_full v) 
			{

                v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
               	o.normal = v.normal;

				float effectStrength = 0;
				
				// For each vertex, get the strongest (non-additive) effect value from the buffered hit positions
				float dist = (distance (v.vertex, _LocalHitPoint0.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint0.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint1.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint1.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint2.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint2.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint3.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint3.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint4.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint4.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint5.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint5.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint6.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint6.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint7.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint7.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint8.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint8.w, dist), effectStrength), 0.0, 1.0;

				dist = (distance (v.vertex, _LocalHitPoint9.xyz) * _MeshScale)/(_SpreadFactor * _MeshScale);
				effectStrength = max(GetEffectValue(_LocalHitPoint9.w, dist), effectStrength), 0.0, 1.0;

				o.effectStrength = effectStrength;
				
                return o;

            }

            half4 frag (v2f f) : COLOR
            {

				// Get the texture value
				half4 tex = tex2D (_MainTex, f.uv) *_Color;
			
				// Get the average of the texture rgb values
				float val = (tex.r + tex.g + tex.b)/3;

				// Get the color of the fragment according to the effect strength at that point
				float alpha = f.effectStrength;
				
				// Multiply the effect color by the texture
				half4 c = tex * alpha;

                return c;
            }

            ENDCG
        }
    }
    Fallback "Transparent/VertexLit"
}
