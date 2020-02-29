Shader "Unlit/AlphaBlend"
{
	Properties
	{
		_Color ("Color",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaScale ("AlphaScale",Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "IgnoreProjector" = "True"  "RenderType"="Transparent" }
		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _AlphaScale;

			struct a2v
			{
				float4 vertex :POSITION;
				float3 normal :NORMAL;
				float4 texcoord :TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float2 uv :TEXCOORD2;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.worldPos = UnityWorldSpaceLightDir(v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

				// o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);//缩放与平移
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 albedo = tex2D(_MainTex,i.uv)*_Color.rgb;

				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;	
				fixed3 worldLight = i.worldPos;
				fixed3 diffuse = _LightColor0.rgb * albedo * saturate(dot(i.worldNormal,worldLight));
				
				fixed3 color = ambient + diffuse;

				return fixed4(color,_Color.a * _AlphaScale);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
