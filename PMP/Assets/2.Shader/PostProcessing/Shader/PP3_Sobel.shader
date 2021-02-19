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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half2 uv[9] : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            half4 _MainTex_TexelSize;
            fixed _EdgeSlider;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;


            v2f vert (appdata v)
            {
                const half2 BOX_9[9] =
                {
                    half2(-1,1),half2(0,1),half2(1,1),
                    half2(-1,0),half2(0,0),half2(1,0),
                    half2(-1,-1),half2(0,-1),half2(1,-1),
                };

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                for(int i = 0;i<9;i++)
                {
                   o.uv[i] = v.uv + _MainTex_TexelSize.xy * BOX_9[i];
                }
                return o;
            }


            half Sobel(v2f i)
            {

                const half Gy[9] = 
                {
                    -1,-2,-1,
                    0, 0, 0,
                    1, 2, 1,
                };

                const half Gx[9] = 
                {
                    -1, 0, 1,
                    -2, 0, 2,
                    -1, 0, 1,
                };


                half texColor;
                half eX = 0;
                half eY = 0;
                for(int n = 0;n<9;n++)
                {
                    texColor = Luminance(tex2D(_MainTex,i.uv[n]));
                    eX += texColor * Gx[n];
                    eY += texColor * Gy[n];
                }
                return 1 - (abs(eX) + abs(eY));
            }

            

            fixed4 frag (v2f i) : SV_Target
            {
                half edge = Sobel(i);
                fixed4 withEdgeColor = lerp(_EdgeColor,tex2D(_MainTex,i.uv[4]),edge);
                fixed4 onlyEdgeColor = lerp(_EdgeColor,_BackgroundColor,edge);
                return lerp(withEdgeColor,onlyEdgeColor,_EdgeSlider);
            }



            ENDCG
        }
    }
    Fallback OFF
}
