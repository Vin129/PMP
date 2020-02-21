// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SimpleShader"
{
	Properties{
		_Color ("这是一个颜色变量",Color) = (1,1,1,1)
		_Vector ("这是一个Vector变量",Vector) = (1,1,1,1)
		_Range ("这是一个范围变量",Range(1,10)) = 1
		// _Float ("这是一个浮点数",Float) = 0.1f
		// _2D ("这是一个2D变量",2D) = sampler2D
		// _3D ("这是一个3D变量",3D) = sampler3D
		// _Cube ("这是一个Cube变量",Cube) = samplerCUBE
	}

	SubShader
	{
		Pass
		{
				CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vert 
				#pragma fragment frag

				// float4 vert(float4 v:POSITION):SV_POSITION
				// {
				// 	return UnityObjectToClipPos(v);
				// }

				float4 vert(appdata_tan t):SV_POSITION
				{
					return UnityObjectToClipPos(t.vertex);
				}

				fixed4 frag():SV_TARGET
				{
					return fixed4(1,1,0,1);
				}

				ENDCG	
		}
	}
}
