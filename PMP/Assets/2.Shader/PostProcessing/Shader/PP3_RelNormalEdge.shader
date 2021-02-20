Shader "PP/PP3_RelNormalEdge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NdotVThreshold ("NdotVThreshold",Range(0,1)) = 0.01
        _EdgeSlider ("EdgeSlider",Range(0,1)) = 1
        _EdgeColor ("EdgeColor",Color) = (0,0,0,1)
        _BackgroundColor ("BackgroundColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        // Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal:NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir :TEXCOORD1;
                float3 normal:Normal;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                return o;
            }

            sampler2D _MainTex;
            float _EdgeSlider;
            float _NdotVThreshold;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 relCol = tex2D(_MainTex, i.uv);
                float NdotV = 1 - dot(i.normal,i.viewDir);

                if(NdotV > _NdotVThreshold)
                    return _EdgeColor;
                else
                    return lerp(_BackgroundColor,relCol,_EdgeSlider);
   
            }
            ENDCG
        }
    }
}
