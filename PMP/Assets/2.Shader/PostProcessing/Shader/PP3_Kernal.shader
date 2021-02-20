Shader "PP/PP3_Kernal"
{
    //卷积能做的事情：锐化、模糊、描边
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeSlider ("EdgeSlider",Range(0,1)) = 1
        _EdgeColor ("EdgeColor",Color) = (0,0,0,1)
        _BackgroundColor ("BackgroundColor",Color) = (1,1,1,1)

        _Factor ("Factor",Float) = 1
        _Kernal00 ("Kernal00",Float) = 1
        _Kernal01 ("Kernal01",Float) = 1
        _Kernal02 ("Kernal02",Float) = 1
        _Kernal10 ("Kernal10",Float) = 1
        _Kernal11 ("Kernal11",Float) = -8
        _Kernal12 ("Kernal12",Float) = 1
        _Kernal20 ("Kernal20",Float) = 1
        _Kernal21 ("Kernal21",Float) = 1
        _Kernal22 ("Kernal22",Float) = 1
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
            Float _Factor;
            Float _Kernal00;
            Float _Kernal01;
            Float _Kernal02;
            Float _Kernal10;
            Float _Kernal11;
            Float _Kernal12;
            Float _Kernal20;
            Float _Kernal21;
            Float _Kernal22;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                VES_Get_Box9_UV(v.uv,_MainTex_TexelSize,o.uv);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                const half K[9] = 
                {
                    _Kernal00,_Kernal01,_Kernal02,
                    _Kernal10, _Kernal11, _Kernal12,
                    _Kernal20, _Kernal21, _Kernal22,
                };


                float2 box25[25];
                VES_Get_Box25_UV(i.uv[4],_MainTex_TexelSize,box25);

                if(_EdgeSlider > i.uv[4].y)
                {
                    // fixed4 col = VES_Kernal(K,i.uv,_Factor,_MainTex);
                    // fixed4 col = VES_Kernal_Blur(i.uv,_MainTex);

                    // fixed4 col = VES_Kernal_GaussianBlur(box25,_MainTex);
                    fixed4 col = VES_Kernal_UnsharpMasking(box25,_MainTex);

                    return col;
                }
                else
                   return tex2D(_MainTex,i.uv[4]);
            }



            ENDCG
        }
    }
    Fallback OFF
}
