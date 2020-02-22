Shader "Unlit/SpecularPixeLevel"
{
	Properties
	{
		_Diffuse ("Color", Color) = (1,1,1,1)
		_Specular ("高光反射颜色",Color) = (1,1,1,1)
		_Gloss ("高光区域大小",Range(8,256)) = 20
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;

			struct a2v
			{
				float4 vertex :POSITION;
				float3 normal :NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos:TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.worldPos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = normalize(mul(v.normal,(float3x3)unity_WorldToObject));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;	
				fixed3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(i.worldNormal,worldLight));
				
				fixed3 reflectDir = normalize(reflect(-worldLight,i.worldNormal));
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld,i.worldPos));
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(viewDir,reflectDir)),_Gloss);
				
				fixed3 color = ambient + diffuse + specular;

				return fixed4(color,1);
			}
			ENDCG
		}	
	}
	Fallback "Specular"
}
