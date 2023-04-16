Shader "RO/Role/PartOutline" {
	Properties 
	{
		_MainTex ("Albedo", 2D) = "white" {}
		_OutlineColor ("Outline Color", Color) = (0.188235,0.121569,0.121569,1)
		_OutlineWidth ("Outline Width", Range(0.0,0.1)) = 0.013

		[HideInInspector] _SubTex ("Sub Albedo", 2D) = "grey" {}
		[HideInInspector] _BlinkSpeed ("Blink Speed", Range(0,10)) = 1
		[HideInInspector] _BlinkMin ("Blink Min", Range(-3,3)) = 0
		[HideInInspector] _BlinkMax ("Blink Max", Range(-3,3)) = 1
		
		[HideInInspector] _Alpha("Alpha", Range(0,1))=1
		[HideInInspector] _ChangeColor ("ChangeColor", Color) = (1,1,1,0)
		[HideInInspector] _MaskColor ("MaskColor", Color) = (1,1,1,0)
		[HideInInspector] _MaskColorThreshold ("MaskColorThreshold", Range(0,1)) = 1
		[HideInInspector] _Mask2Color("Mask2Color", Color) = (0,0,1,0)
		[HideInInspector] _SatValue("SatValue",  Range(0,2)) = 1.0


		[HideInInspector] _LightColor1 ("Light Color 1", Color) = (1,1,1,1)
		[HideInInspector] _Exposure1 ("Exposure1", Range(-3,3)) = 0.5
		[HideInInspector] _LightColor2 ("Light Color 2", Color) = (1,1,1,1)
		[HideInInspector] _Exposure2 ("Exposure2", Range(-3,3)) = -0.5
		[HideInInspector] _LightColor3 ("Light Color 3", Color) = (1,1,1,1)
		[HideInInspector] _Exposure3 ("Exposure3", Range(-3,3)) = 0
		[HideInInspector] _LightDirection1 ("Light Direction 1", Vector) = (0.4484,-0.15643,-0.88002,0)
		[HideInInspector] _LightDirection2 ("Light Direction 2", Vector) = (-0.5752,0.27563,-0.7701,0)
		[HideInInspector] _ToonEffect("Toon Effect", Range(0,1))=0.5
		[HideInInspector] _ToonSteps("Toon Steps", Range(0,9))=1
		
		[HideInInspector] _MultiColorNumber("Multi Color Number", Float)=-1
		
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

	}
	
	CGINCLUDE
	#include "RolePartHelper.cginc"
	ENDCG

	SubShader {
		Tags { "IGNOREPROJECTOR"="true" }
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		
		Pass 
		{
			Name "OUTLINE"
			Tags { "Queue" = "Transparent"} 
			
//			Cull Front
//			ZWrite Off
//			ZTest Always
//			ColorMask RGB

			Cull Front
			ZWrite On
			ColorMask RGB
			//Offset 1,1

			CGPROGRAM
			
			#pragma multi_compile __ _MaskColor_ON
			#pragma multi_compile __ _Mask2Color_ON
			#pragma multi_compile __ _ToonLight_ON
			#pragma multi_compile __ _ChangeColor_ON 
			#pragma multi_compile __ _Alpha_ON
			#pragma multi_compile __ _ALPHATEST_ON
			#pragma multi_compile __ _BLINK_ON
			#pragma multi_compile __ _SAT_ON
			#pragma vertex vert_Outline
			#pragma fragment frag_Outline
			#pragma multi_compile_fog
			
			ENDCG
		}
		
		// shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2ff { 
                V2F_SHADOW_CASTER;
            };

            v2ff vert(appdata_base v)
            {
                v2ff o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 fragg(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

		UsePass "RO/Role/Part/BASE"
	} 
	CustomEditor "RolePartShaderGUI"
}
