Shader "PP/PP2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Slider ("Slider",Range(0,1)) = 0
        _DepthParam("DepthParam",Range(0,1)) = 1
        _SliderWidth ("SliderWidth",Range(0,1)) = 0
    }
    SubShader
    {
        // ZWrite On 
        // ZTest Always

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
                float depth:DEPTH;
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _DepthParam;
            float _Slider;
            float _SliderWidth;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // o.depth = -(mul(UNITY_MATRIX_MV,v.vertex).z) * _ProjectionParams.w;
                o.depth = COMPUTE_DEPTH_01;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dp = pow(i.depth,_DepthParam);
                if(_SliderWidth == 0)
                {
                    if(dp > pow(_Slider,_DepthParam))
                        return float4(1 - dp,1 - dp,1 - dp,1);
                    else
                        return tex2D(_MainTex,i.uv);
                }
                else
                {
                    float mmin = max(_Slider - _SliderWidth/2,0);
                    float mmax = min(_Slider + _SliderWidth/2,1);
                    if(pow(mmin,_DepthParam) < dp && dp < pow(mmax,_DepthParam))
                    {
                        // return float4(1 - dp,1 - dp,1 - dp,1);
                        fixed4 col = tex2D(_MainTex,i.uv);
                        return float4(1-col.r,1-col.g,1-col.b,1);
                    }
                    else if (dp < pow(mmin,_DepthParam))
                    {
                        return float4(1 - dp,1 - dp,1 - dp,1);
                    }
                    else
                        return tex2D(_MainTex,i.uv);
                }
            }
            ENDCG
        }
    }
}
