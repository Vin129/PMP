Shader "Unlit/Tex/SimpleTex"
{
	Properties
	{
		_Color ("Color",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _Specular;
			float _Gloss;

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
				o.worldPos = v.vertex;
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
				fixed3 worldLight = UnityWorldSpaceLightDir(i.worldPos);
				fixed3 diffuse = _LightColor0.rgb * albedo * saturate(dot(i.worldNormal,worldLight));
				
				
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 blinnDir = normalize(viewDir + worldLight);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(i.worldNormal,blinnDir)),_Gloss);
				
				fixed3 color = ambient + diffuse + specular;

				return fixed4(color,1);
			}
			ENDCG
		}	
	}
	Fallback "Specular"
}
