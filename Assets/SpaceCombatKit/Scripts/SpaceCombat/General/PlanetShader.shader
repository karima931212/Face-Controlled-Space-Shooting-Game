// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/PlanetShader"
{
    Properties
    {
        _MainTex1("Albedo", 2D) = "white" {}
		_MainTex2("Albedo", 2D) = "white" {}

		_Normal1("Normal 1", 2D) = "bump" {}
		_Normal2("Normal 2", 2D) = "bump" {}
		_NormalStrength("Normal Strength", float) = 0

		_Color1("Color 1", Color) = (0, 0, 0, 1)
        _Color2("Color 2", Color) = (0, 0, 0, 1)

		_SplatMap("Splat Map", 2D) = "white" {}

		_CameraWorldPos("Camera World Position", Vector) = (0,0,0,1)
        
		_Emissive("Emissive", 2D) = "black" {}
		_EmissiveColor("Emissive Color", Color) = (0.5, 0.5, 0.5, 1)
		_AtmoColor("Atmosphere Color", Color) = (0.5, 0.5, 1.0, 1)
        _AtmoZenithColor("Atmosphere Zenith Color", Color) = (0.5, 0.5, 1.0, 1)
		_Size("Size", Float) = 0.1
		_HorizonNonRimness("Horizon Non-Rimness", Float) = 0
        _Falloff("Falloff", Float) = 5
		_FalloffCompression("Falloff Compression", Float) = 4
        _FalloffPlanet("Falloff Planet", Float) = 5
        _Transparency("Transparency", Float) = 15
        _SurfaceAtmosphereAlpha("Surface Atmosphere Alpha", Float) = 1
		_AmbientMultiplier("Ambient Multiplier", Float) = 1
    }
 
	Subshader {
      	
		Tags {"LightMode" = "ForwardBase"}
		//Tags {"RenderType" = "Opaque"}

		// This pass simulates the atmosphere 'inside' the silhouette of the planet by fading out the atmosphere as you move away
		// from the rim, inward toward the planet center.

        Pass
        {
           // Name "PlanetBase"
            //Cull Back
 
            CGPROGRAM
			#pragma vertex vert
			
            #pragma fragment frag

            #pragma fragmentoption ARB_fog_exp2
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0
			


			sampler2D _SplatMap;
			float4 _SplatMap_ST;

			sampler2D _MainTex1;
			float4 _MainTex1_ST;

			sampler2D _MainTex2;
			float4 _MainTex2_ST;

			sampler2D _Normal1;
			float4 _Normal1_ST;

			sampler2D _Normal2;
			float4 _Normal2_ST;

			uniform float _NormalStrength;

			sampler2D _Emissive;

			// Variables must be declared for each pass
            uniform float4 _Color1;
			uniform float4 _Color2;
			
		 	uniform float4 _AtmoZenithColor;
            uniform float4 _AtmoColor;
            uniform float _FalloffPlanet;
            uniform float _SurfaceAtmosphereAlpha;

			uniform float _HorizonNonRimness;

			uniform float4 _CameraWorldPos;

			uniform float _AmbientMultiplier;
			
			uniform float4 _EmissiveColor;

			
            struct v2f
            {

                float2 uv : TEXCOORD0;
				float3 worldPos: TEXCOORD1;
                fixed4 diff : COLOR0;
                float4 vertex : SV_POSITION;
				float3 normal: NORMAL;
				
				float2 uv_MainTex1 : TEXCOORD2;
				float2 uv_MainTex2 : TEXCOORD3;

				float2 uv_Normal1 : TEXCOORD4;
				float2 uv_Normal2 : TEXCOORD5;

				float2 uv_SplatMap : TEXCOORD6;

				half3 tspace0 : TEXCOORD7;
				half3 tspace1 : TEXCOORD8;
				half3 tspace2 : TEXCOORD9;

            };

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);	// convert the normal from object to world space

                o.uv = v.texcoord;

				o.uv_MainTex1 = TRANSFORM_TEX(v.texcoord, _MainTex1);
				o.uv_MainTex2 = TRANSFORM_TEX(v.texcoord, _MainTex2);
	
				o.uv_Normal1 = TRANSFORM_TEX(v.texcoord, _Normal1);
				o.uv_Normal2 = TRANSFORM_TEX(v.texcoord, _Normal2);

				o.uv_SplatMap = TRANSFORM_TEX(v.texcoord, _SplatMap);

				// Convert the vertex normal from object to world space
				half3 wNormal = UnityObjectToWorldNormal(v.normal);

				// Convert the tangent from object to world space
				half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);

				// Compute bitangent from cross product of normal and tangent (cross product produces vector orthogonal to both inputs)
				// so bitangent is just another tangent vector rotated around the normal by 90 degrees
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross (wNormal, wTangent) * tangentSign;

				// Create the tangent matrix
				o.tspace0 = half3 (wTangent.x, wBitangent.x, wNormal.x);
				o.tspace1 = half3 (wTangent.y, wBitangent.y, wNormal.y);
				o.tspace2 = half3 (wTangent.z, wBitangent.z, wNormal.z);

				// Calculate ambient lighting
                half nl = max(0, dot(wNormal, _WorldSpaceLightPos0.xyz));
                //o.diff = nl * _LightColor0;
 				o.diff = half4(ShadeSH9(half4(wNormal,1)),1);
                return o;
            }
            
            

            fixed4 frag (v2f i) : SV_Target
            {

				// Sample the normal map
				half3 tNormal = UnpackNormal (tex2D (_Normal1, i.uv_Normal1));

				// Change the strength of the normal by reducing the z-value and then normalizing (flattening the angle). 
				// Note normal is a vector NOT a color
				tNormal.z *= _NormalStrength;
				tNormal = normalize(tNormal);
				
				// convert the tNormal (tangent space normal as normal maps are) to world normal, using the projection
				// matrix defined in the vertex program
				half3 worldNormal;
				worldNormal.x = dot (i.tspace0, tNormal);
				worldNormal.y = dot (i.tspace1, tNormal);
				worldNormal.z = dot (i.tspace2, tNormal);

				// Get the tinted texture color
				half4 tintedTextureColor1 = tex2D(_MainTex1, i.uv_MainTex1) * _Color1;
				half4 tintedTextureColor2 = tex2D(_MainTex2, i.uv_MainTex2) * _Color2;
				float splatVal = saturate(tex2D (_SplatMap, i.uv_SplatMap).r);
				half4 tintedTextureColor = splatVal * tintedTextureColor2 + (1 - splatVal) * tintedTextureColor1;
				tintedTextureColor *= _Color1;
				
				// Calculate the direct lighting value
				half4 lightingVal = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));

				// Add the ambient lighting value
				lightingVal += i.diff;

				// Calculate the atmosphere color
				float3 viewdir = normalize(_CameraWorldPos-i.worldPos);							// Get the direction from the fragment position to the camera
				float strength = pow(1.0-saturate(dot(viewdir, worldNormal)), _FalloffPlanet);	// Get the atmosphere strength (rimness) of the fragment
				float4 atmoColor = strength * _AtmoZenithColor + (1-strength) * _AtmoColor;		// Lerp from atmosphere to zenith color toward horizon
				
				half4 c = strength * atmoColor + (1-strength) * tintedTextureColor;

				c *= lightingVal;

				// Add emissive light
				c += tex2D(_Emissive, i.uv_MainTex1) * _EmissiveColor;

				// Normalize the value
				c = saturate(c);
				
             	return c;
			}
          	ENDCG	
		}

		// This pass extrudes the surface of the planet outward and shades it with the atmosphere color (fading in as you move away from the rim)
		// which is the opposite of the first pass, in order to simulate atmosphere fading out away from the surface

		// This is done simply by inverting the view vector to be camera-> fragment instead of fragment->camera when calculating 'rimness'
		
		Tags {"LightMode" = "ForwardBase" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass
        {
            Name "FORWARD"
            Cull Front
            Blend SrcAlpha One
			ZWrite Off
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma fragmentoption ARB_fog_exp2
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc" // for _LightColor0
			
			// Variables must be declared for each pass
            uniform float4 _Color;
			uniform float4 _AtmoZenithColor;
            uniform float4 _AtmoColor;
			uniform float4 _CameraWorldPos;
            uniform float _Size;
			uniform float _HorizonNonRimness;
            uniform float _Falloff;
			uniform float _FalloffCompression;
            uniform float _Transparency;
			uniform float _AmbientMultiplier;
			
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldvertpos : TEXCOORD1;
				fixed4 diff : COLOR0; // diffuse lighting color
            };

            v2f vert(appdata_base v)
            {
                v2f o;

				// Note: /1000 is just a way to make it easier to control with a slider
           		v.vertex.xyz += v.normal*(_Size/1000);							// Extrude the vertex along its normal by _Size (thickness of atmosphere)
                o.pos = UnityObjectToClipPos (v.vertex);				// Convert the vertex position from world to screen coordinates
				o.normal = mul((float3x3)unity_ObjectToWorld, v.normal); 		// convert the normal from object to world space
                o.worldvertpos = mul(unity_ObjectToWorld, v.vertex);			// Set the world position, by converting from object to world space
				
				// get vertex normal in world space
                //half3 worldNormal = UnityObjectToWorldNormal(v.normal);	// Get the normal in world space
				//o.diff.rgb = ShadeSH9(half4(worldNormal,1));			// Calculate the ambient light again for this pass

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
 
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));

                return o;
            }

			float4 frag(v2f i) : COLOR
            {
                i.normal = normalize(i.normal);											// Get the direction of the normal
                float3 viewdir = normalize(i.worldvertpos - _CameraWorldPos);		// Get the direction from the camera to vertex position (inverted of first pass!)
				float non_rimness = dot(viewdir, i.normal);

				float factor = 6;
				float zenithAmount = saturate((saturate(non_rimness - (_HorizonNonRimness-(_HorizonNonRimness/factor))) * factor)/_HorizonNonRimness); //saturate(non_rimness/horizonNonRimness);
				float4 color = zenithAmount * _AtmoZenithColor + (1-zenithAmount) * _AtmoColor;
				color.a = pow(non_rimness, 4);
				
				//color.a = pow ((color.a*_FalloffCompression), _Falloff);				// Set the falloff
				//color.a *= saturate(dot(i.normal, _WorldSpaceLightPos0)) + i.diff*_AmbientMultiplier;		// Fade in the color according to how much light is incident on the surface plus ambient light
				color.a *= _Transparency;												// Set the transparency as desired
				color *= i.diff;
				color = saturate(color);
				return color;
            }
            ENDCG
        }
    }
 
    FallBack "Diffuse"
}