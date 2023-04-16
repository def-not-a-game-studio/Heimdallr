// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RO/SceneObject/Lit" {
	Properties 
	{
		[HideInInspector] _MainTex ("Albedo", 2D) = "white" {}
		[HideInInspector] _Color("Color", Color) = (1,1,1,1) 
		[HideInInspector] _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

		[HideInInspector] _CutX ("__cutx", Float) = 0.0
		[HideInInspector] _CutY ("__cuty", Float) = 0.0

	    [Toggle(_NO_FOG_ON)]_NoFog ("No Fog", Float) = 0
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	struct appdata_scene
	{
		half4 vertex : POSITION;
		half4 texcoord : TEXCOORD0;
		#ifndef LIGHTMAP_OFF
		half4 texcoord1 : TEXCOORD1;
		#endif
//		#if defined(_PointLight_ON)
//		half3 normal : NORMAL;
//		#endif
	};
	
	struct v2f
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		#ifndef LIGHTMAP_OFF
		half2 lmap : TEXCOORD1;
		#endif
		UNITY_FOG_COORDS(2)
//		#if defined(_PointLight_ON)
//		half4 posWorld : TEXCOORD3;
//		half3 normalDir : TEXCOORD4;
//		#endif
	};
	
	uniform sampler2D _MainTex;
	uniform half4 _MainTex_ST;
	uniform half4 _Color;
	uniform half _Cutoff;
	uniform half _CutX;
	uniform half _CutY;
	
	uniform half4 _PointLightColor;
	uniform half _PointLightIntensity;
	uniform half4 _PointLightPos;
	
	ENDCG

	SubShader {
		Tags { }
		
		Pass {
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			
			#pragma multi_compile __ _ALPHATEST_ON
//			#pragma multi_compile __ _ALPHABLEND_ON
//			#pragma multi_compile __ _ALPHAPREMULTIPLY_ON
//			#pragma multi_compile __ _PointLight_ON
			#pragma multi_compile __ _NO_FOG_ON
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			v2f vert(appdata_scene v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				#ifndef LIGHTMAP_OFF
				o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#if !defined(_NO_FOG_ON)
				UNITY_TRANSFER_FOG(o, o.pos);
				#endif
				
//				#if defined(_PointLight_ON)
//				o.posWorld = mul(_Object2World, v.vertex);
//				o.normalDir =  normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);  
//				#endif
				return o;
			}
			
				half4 frag(v2f i) : COLOR {							 		
				half4 c = tex2D (_MainTex, i.uv);
				c.rgb *= _Color.rgb * 0.78;
				
				#if defined(_ALPHATEST_ON)
				clip (c.a - _Cutoff);
				#endif
				
				#ifndef LIGHTMAP_OFF
				half3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
				c.rgb *= lm;
				#endif
				
				half4 lightColor = UNITY_LIGHTMODEL_AMBIENT;
				c.rgb *= lightColor.rgb * lightColor.a;
				
//				#if defined(_PointLight_ON)
//				float3 normalDirection = normalize(i.normalDir); 
//				half3 vertexToLightSource = _PointLightPos.xyz - i.posWorld.xyz;  
//                half dist = length(vertexToLightSource);  
//                half attenuation = 1.0 / dist;
//                half3 lightDirection = normalize(vertexToLightSource);  
//                  
//                // diffuseReflection
//                c.rgb += attenuation * _PointLightColor.rgb * c.rgb * max(0.0, dot(normalDirection, lightDirection)) * _PointLightIntensity;  
//				#endif

				#if !defined(_NO_FOG_ON)
				UNITY_APPLY_FOG(i.fogCoord, c);
				#endif

				#if defined(_CUT_ON)
				if (_CutY > i.uv.y || (1-_CutY) < i.uv.y)
				{
					c.a = 0;
				}
				else if (_CutX > i.uv.x || (1-_CutX) < i.uv.x)
				{
					c.a = 0;
				}
				#endif
				return c;			
			}
			ENDCG
		}
	} 
	FallBack "Standard"
	CustomEditor "SceneObjectShaderGUI"
}
