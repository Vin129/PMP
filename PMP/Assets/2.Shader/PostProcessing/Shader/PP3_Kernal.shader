Shader "PP/PP3_Kernal"
{
    //基于图像的边缘检测
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


            fixed4 Kernal(v2f i)
            {
                const half K[9] = 
                {
                    _Kernal00,_Kernal01,_Kernal02,
                    _Kernal10, _Kernal11, _Kernal12,
                    _Kernal20, _Kernal21, _Kernal22,
                };
                fixed4 color = (0,0,0,0);
                for(int n = 0;n<9;n++)
                {
                    color += tex2D(_MainTex,i.uv[n]) * K[n];
                }
                return color/_Factor;
            }

            

            fixed4 frag (v2f i) : SV_Target
            {
                if(_EdgeSlider > i.uv[4].y)
                {
                    fixed4 col = Kernal(i);
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
