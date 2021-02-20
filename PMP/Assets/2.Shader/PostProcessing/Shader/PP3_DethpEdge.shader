Shader "PP/PP3_DethpEdge"
{
    //基于深度与法线的边缘检测
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeSlider ("EdgeSlider",Range(0,1)) = 1
        _DepthThreshold ("DepthThreshold",Range(0,1)) = 0.002
        _NdotVThreshold ("NdotVThreshold",Range(0,1)) = 0.01

        _EdgeColor ("EdgeColor",Color) = (0,0,0,1)
        _BackgroundColor ("BackgroundColor",Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
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
                float3 viewDir : TEXCOORD9;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            half4 _MainTex_TexelSize;
            sampler2D _CameraDepthNormalsTexture;
            half4 _CameraDepthNormalsTexture_TexelSize;
            float _EdgeSlider;
            float _DepthThreshold;
            float _NdotVThreshold;
            fixed4 _EdgeColor;
            fixed4 _BackgroundColor;

            void ComputeDethpNormal(v2f i,int index,out float depth,out fixed3 normal)
            {
                fixed4 col = tex2D(_CameraDepthNormalsTexture, i.uv[index]);
                DecodeDepthNormal(col,depth,normal);
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                VES_Get_Box9_UV(v.uv,_CameraDepthNormalsTexture_TexelSize,o.uv);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float depth = 0;
                fixed3 normal;

                fixed4 relCol = tex2D(_MainTex,i.uv[4]);
                float relDepth;
                fixed3 relNormal;
                ComputeDethpNormal(i,4,relDepth,relNormal);

                for(int n = 0;n<9;n++)
                {
                    if(n !=4)
                    {
                        float _depth;
                        float3 _normal; 
                        ComputeDethpNormal(i,n,_depth,_normal);
                        depth += _depth;
                        if(n == 0)
                            normal = _normal;
                        else
                            normal -= _normal;
                    }
                }


                float depthDiff = abs(relDepth - depth/8);  //深度差异在于采样点深度相差值
                normal = normalize(normal);  //法线差异在于四周采样点的法线是否是相互平行的，去差值来观察偏差
                float normalDiff = abs(normal.x + normal.y);

                // float NdotV = 1 - dot(normal,i.viewDir);//法线方向与视角方向垂直越可能是边界



                if(depthDiff > _DepthThreshold * 0.01)
                {
                    return _EdgeColor;
                }
                else
                    return lerp(_BackgroundColor,relCol,_EdgeSlider);

                //
                // if(normalDiff*depthDiff > _NdotVThreshold * 0.001)
                // {
                //     return _EdgeColor;
                // }
                // else
                //     return lerp(relCol,_BackgroundColor,_EdgeSlider);


                // return fixed4(normalDiff,normalDiff,normalDiff,1);

                // return col;
            }
            ENDCG
        }
    }
}
