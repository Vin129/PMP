Shader "PP/PP1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Slider ("Slider",Range(0,2)) = 1
        _bGray ("bGray",Int) = 0
        _Point ("Point",Vector) = (0.5,0.5,1)
    }
    SubShader
    {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            fixed _Slider;
            fixed _bGray;
            vector _Point;

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                // if(i.uv.x > _SinTime.w)
                // if(i.uv.x > _Slider)
                if(distance(i.uv.xy,_Point) >= _Slider  )
                {
                    if(_bGray == 1)
                    {
                        fixed grayCol = dot(col.rgb,fixed3(0.212,0.515,0.12));
                        return float4(grayCol,grayCol,grayCol,1);
                    }
                    else
                        return float4(1-col.r,1-col.g,1-col.b,1);
                }
                else
                    return col;
                
            }
            ENDCG
        }
    }
}
