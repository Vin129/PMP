Shader "PP/PP3_Sobel"
{
    //基于图像的边缘检测
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeSlider ("EdgeSlider",Range(0,1)) = 1
        _EdgeColor ("EdgeColor",Color) = (0,0,0,1)
        _BackgroundColor ("BackgroundColor",Color) = (1,1,1,1)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "PP_Utils.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv[9] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            half4 _MainTex_TexelSize;
            fixed _EdgeSlider;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                VES_Get_Box9_UV(v.uv,_MainTex_TexelSize,o.uv);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half edge = VES_Sobel(i.uv,_MainTex);
                fixed4 withEdgeColor = lerp(_EdgeColor,tex2D(_MainTex,i.uv[4]),edge);
                fixed4 onlyEdgeColor = lerp(_EdgeColor,_BackgroundColor,edge);
                return lerp(withEdgeColor,onlyEdgeColor,_EdgeSlider);
            }

            ENDCG
        }
    }
    Fallback OFF
}
