Shader "Unlit/FlaseShader"
{
	Properties
	{
		_FlaseType ("Type",Int) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			int _FlaseType;

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};
	
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				if(_FlaseType == 1)
				{
					o.color = fixed4(v.normal * 0.5 + fixed3(0.5,0.5,0.5),1);//法线
				}
				else if(_FlaseType == 2){
					o.color = fixed4(v.tangent.xyz * 0.5 + fixed3(0.5,0.5,0.5),1);//切线
				}
				else if(_FlaseType == 3){
					fixed3 binormal = cross(v.normal,v.tangent.xyz) * v.tangent.w;
					o.color = fixed4(binormal * 0.5 + fixed3(0.5,0.5,0.5),1);//副切线
				}
				else if(_FlaseType == 4){
					o.color = fixed4(v.texcoord.xy ,0,1);//第一组纹理
				}

				return o;
			}
			
			fixed4 frag (v2f o) : SV_Target
			{
				return o.color;
			}
			ENDCG
		}
	}
}
