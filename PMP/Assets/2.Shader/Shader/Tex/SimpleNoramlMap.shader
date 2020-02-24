Shader "Unlit/Tex/SimpleNoramlMap"
{
Properties
	{
		_Color ("Color",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("法线纹理",2D) = "bump"{}
		_BumpScale ("凹凸影响程度",Range(0,10)) = 1
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
			sampler2D _BumpMap;
			float _BumpScale;
			float4 _BumpMap_ST;
			fixed4 _Color;
			fixed4 _Specular;
			float _Gloss;

			struct a2v
			{
				float4 vertex :POSITION;
				float3 normal :NORMAL;
				float4 texcoord :TEXCOORD0;
				float4 tangent :TANGENT;//切线方向
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv :TEXCOORD2; //两个纹理要存在这里
				float3 lightDir : TEXCOORD0; // 计算切线空间坐标下的光线和视角
				float3 viewDir :TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord,_BumpMap);

				//把模型空间下切线方向，副切线方向，法线方向 排列得到模型空间到切线空间的旋转矩阵
				//副切线由其他两个做叉乘得到，乘w 是为了区分方向（因为有两个）
				// float3 binormal = cross(normalize(v.normal),normalize(v.tangent.xyz))*v.tangent.w;
				// float3x3 rotation = float3x3(v.tangent.xyz,binormal,v.normal);
				// TANGENT_SPACE_ROTATION;//内置宏,效果同上
				TANGENT_SPACE_ROTATION;
				o.lightDir = mul(rotation,ObjSpaceLightDir(v.vertex)); //模型——>切线
				o.viewDir = mul(rotation,ObjSpaceViewDir(v.vertex));
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 tangentLightDir = normalize(i.lightDir);
				fixed3 tangentViewDir = normalize(i.viewDir);

				fixed4 packedNormal = tex2D(_BumpMap,i.uv.zw);
				fixed3 tangentNromalDir;
				tangentNromalDir = UnpackNormal(packedNormal);//法线纹理设置成 normal map 后使用此方法
				// tangentNromalDir.xy = (packedNormal*2 - 1)*_BumpScale;
				tangentNromalDir.xy *= _BumpScale;
				tangentNromalDir.z = sqrt(1- saturate(dot(tangentNromalDir.xy,tangentNromalDir.xy)));//根据xy推导z


				fixed3 albedo = tex2D(_MainTex,i.uv.xy)*_Color.rgb;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;	
				fixed3 diffuse = _LightColor0.rgb * albedo * saturate(dot(tangentNromalDir,tangentLightDir));
				
				fixed3 blinnDir = normalize(tangentViewDir + tangentLightDir);
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(tangentNromalDir,blinnDir)),_Gloss);
				
				fixed3 color = ambient + diffuse + specular;

				return fixed4(color,1);
			}
			ENDCG
		}	
	}
	Fallback "Specular"
}
