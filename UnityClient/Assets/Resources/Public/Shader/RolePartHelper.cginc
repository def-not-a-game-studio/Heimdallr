// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef ROLE_PART_HELPER_INCLUDED
#define ROLE_PART_HELPER_INCLUDED

#include "UnityCG.cginc"

struct appdata_role
{
	half4 vertex : POSITION;
	half3 normal : NORMAL;
	half4 texcoord : TEXCOORD0;
};

struct v2f
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	half3 normal : TEXCOORD1;
	UNITY_FOG_COORDS(2)
};

uniform sampler2D _MainTex;
#if defined(_BLINK_ON)
uniform sampler2D _SubTex;
uniform half _BlinkSpeed;
uniform half _BlinkMin;
uniform half _BlinkMax;
#endif

// outline
uniform half4 _OutlineColor;
uniform float _OutlineWidth;

// transparent
uniform half _Alpha;

// change color
uniform half4 _ChangeColor;

// mask color
uniform half4 _MaskColor;
uniform float _MaskColorThreshold;

// mask color2
uniform half4 _Mask2Color;

// toon light
uniform half4 _LightColor1;
uniform half _Exposure1;
uniform half4 _LightColor2;
uniform half _Exposure2;
uniform half4 _LightColor3;
uniform half _Exposure3;
uniform half4 _LightDirection1;
uniform half4 _LightDirection2;
uniform half _ToonEffect;
uniform half _ToonSteps;
uniform half _SatValue;;

// multi color
uniform half _MultiColorNumber;

half4 mask_color(half4 c)
{
	half3 oldRGB = c.rgb;
	half k = max(0, c.a - 0.9) * 10;
	half  ce	 = step(k, _MaskColorThreshold);
	half3 mColor = _MaskColor.rgb;
	//half  val = step(c.g, _MaskColorThreshold);
	//half3 mColor = _MaskColor.rgb*(1 - val) + half3(0.4, 0.3, 0.6)*(val);
	////////////////////// 
	half3 hNew = (mColor*c.rrr) / 0.5;
	half3 lNew = 1.0 - (1.0 - mColor)*(1.0 - c.rrr) / 0.5;
	half val = step( c.r, 0.5);
	c.rgb = hNew *val + lNew * (1 - val);
	//////////////////////
	c.rgb = lerp(c.rgb, oldRGB, k /_MaskColorThreshold);
	/////////////////////
	c.rgb = c.rgb*(ce) + oldRGB * (1 - ce);
	/////////////////////
	c.a = 1;
	return c;
}

half4 mask_2color(half4 c)
{
	
	half3 oldRGB = c.rgb;
	half k = max(0, c.a - 0.9) * 10;
	half  ce  = step(k, _MaskColorThreshold);
	half  val = step(_MaskColorThreshold, c.g + 0.6);
	half3 mColor = _MaskColor.rgb*(val) + _Mask2Color.rgb*(1- val);
	////////////////////// 
	half3 hNew = (mColor*c.rrr) / 0.5;
	half3 lNew = 1.0 - (1.0 - mColor)*(1.0 - c.rrr) / 0.5;
	val = step(c.r, 0.5);
	c.rgb = hNew * val + lNew * (1 - val);
	//////////////////////
	c.rgb = lerp(c.rgb,oldRGB, k / _MaskColorThreshold);
	/////////////////////
	c.rgb = c.rgb*(ce)+oldRGB * (1 - ce);
	/////////////////////
	c.a = 1;
	return c;
}

half4 sat_color(half4 c)
{
	half3 lum = Luminance(c.rgb);
	lum = lerp(lum, c.rgb, _SatValue);
	return half4(lum, 1);
}

half3 toon_LightAdd(half3 c, half3 normal)
{
	half diff = max(0, dot(normal, normalize(-_LightDirection1.xyz)));
	if (0 == diff)
	{
		return c + _LightColor3.rgb*_Exposure3;
	}
	else
	{
		diff = smoothstep(-1, 1, diff);
		half toon = floor(diff*_ToonSteps)/_ToonSteps;
		diff = lerp(diff,toon,_ToonEffect);
		return c + _LightColor1.rgb*diff*_Exposure1;
	}
}

v2f vert_Outline(appdata_role v) 
{
	v2f o;
	
	
	
	#if defined(_Alpha_ON)
	o.pos = UnityObjectToClipPos(v.vertex);
	return o;
	#else

	float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
	float3 normal = mul((float3x3)UNITY_MATRIX_MV, v.normal);
	float2 dir = normalize(normal.xy);
	
	pos = pos/pos.w;
	pos.xy = pos.xy + dir * (_OutlineWidth*(abs(pos.z*0.03f) + 0.25f));
	o.pos = mul(UNITY_MATRIX_P, pos);
	//o.pos = o.pos/o.pos.w;
	//o.pos = UnityObjectToClipPos(v.vertex);
	//half3 norm = mul((half3x3)UNITY_MATRIX_IT_MV, v.normal);
	//half4 norm4 = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.normal, 0.0)));
	//half2 dir = normalize(norm4.xy);
	//half2 dir = TransformViewToProjection(norm.xy);
	//o.pos.xy += dir * _OutlineWidth * ((o.pos.z * 0.05) * 0.75+0.25);
	//o.pos.xy += dir * _OutlineWidth *(o.pos.z * 0.375 + 0.25);
	//o.pos.xy += dir * _OutlineWidth * (max(0, (1 - abs(UNITY_NEAR_CLIP_VALUE - o.pos.z/ o.pos.w)))* 5 + 0.25);
	//o.pos.z += _OutlineWidth;
	o.uv     = v.texcoord.xy;
	o.normal = normal;
	UNITY_TRANSFER_FOG(o, o.pos);	
	return o;
	#endif
}

half4 frag_Outline(v2f i) : COLOR 
{
	#if defined(_Alpha_ON)
	return half4(0,0,0,0);
	#else
	
	
	#if defined(_ALPHATEST_ON)
	half4 testCol = tex2D(_MainTex, i.uv);
	clip (testCol.a - 0.5);
	testCol.rgb = _OutlineColor.rgb;
	UNITY_APPLY_FOG(i.fogCoord, testCol);
	return testCol;
	#endif
	half4 c = half4(_OutlineColor.rgb, _OutlineColor.a);
	UNITY_APPLY_FOG(i.fogCoord, c);
	return c;
	#endif
}

v2f vert(appdata_role v) 
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord.xy;
	o.normal = mul((half3x3)unity_ObjectToWorld, v.normal);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

half4 frag(v2f i) : COLOR 
{
	half4 c = tex2D(_MainTex, i.uv);
	//return c.b;
	#if defined(_BLINK_ON)
		half4 subC = tex2D(_SubTex, i.uv);
		float p = (sin(_Time.w*_BlinkSpeed)+1)*0.5;
		subC.a *= p*(_BlinkMax-_BlinkMin)+_BlinkMin;
		c.rgb = lerp(c.rgb, subC.rgb, subC.a);
	#endif
	
	#if defined(_ALPHATEST_ON)
		clip (c.a - 0.5);
	#endif
	
	#if defined(_MaskColor_ON)
		#if defined(_Mask2Color_ON)
			c = mask_2color(c);
		#else
			c = mask_color(c);
		#endif
	#endif
			
	#if defined(_SAT_ON)
		c = sat_color (c);
	#endif
		
	#if defined(_ToonLight_ON)
	c.rgb = toon_LightAdd(c.rgb, normalize(i.normal));
	#endif
	
	#if defined(_ChangeColor_ON)
	c.rgb = lerp(c.rgb, _ChangeColor.rgb, _ChangeColor.a);
	#endif
	
	#if defined(_Alpha_ON)
	c.a *= _Alpha;
	#endif

	UNITY_APPLY_FOG(i.fogCoord, c);
	return c;
}

#endif
