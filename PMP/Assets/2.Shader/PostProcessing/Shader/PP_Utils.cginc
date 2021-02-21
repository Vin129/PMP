#ifndef VE_SHADER_UTILS

#include "UnityCG.cginc"
//=====================================================================//
//Box 采样
void VES_Get_Box5_UV01(float2 uv,half4 unit,out float2 boxuv[5])
{
    const half2 BOX_5_01[5] =
    {
                    half2(0,-1),
        half2(-1,0),half2(0,0),half2(1,0),
                    half2(0,1)
    };

    for(int it = 0;it < 5;it++)
    {
        boxuv[it] = uv + BOX_5_01[it]*unit.xy;
    }
}

void VES_Get_Box5_UV02(float2 uv,half4 unit,out float2 boxuv[5])
{
    const half2 BOX_5_02[5] =
    {
        half2(-1,-1),         half2(-1,-1),
                    half2(0,0),
        half2(-1,1),          half2(1,1)
    };

    for(int it = 0;it < 5;it++)
    {
        boxuv[it] = uv + BOX_5_02[it]*unit.xy;
    }
}

void VES_Get_Box5_UV(float2 uv,half4 unit,out float2 boxuv[5])
{
    VES_Get_Box5_UV01(uv,unit,boxuv);
}


void VES_Get_Box9_UV(float2 uv,half4 unit,out float2 boxuv[9])
{
    const half2 BOX_9[9] =
    {
        half2(-1,-1),half2(0,-1),half2(1,-1),
        half2(-1,0), half2(0,0), half2(1,0),
        half2(-1,1), half2(0,1), half2(1,1)
    };

    for(int it = 0;it < 9;it++)
    {
        boxuv[it] = uv + BOX_9[it]*unit.xy;
    }
}


void VES_Get_Box25_UV(float2 uv,half4 unit,out float2 boxuv[25])
{
    const half2 BOX_25[25] =
    {
        half2(-2,-2),half2(-1,-2),half2(0,-2),half2(1,-2),half2(2,-2),
        half2(-2,-1),half2(-1,-1),half2(0,-1),half2(1,-1),half2(2,-1),
        half2(-2,0),half2(-1,0),half2(0,0),half2(1,0),half2(2,0),
        half2(-2,1),half2(-1,1),half2(0,1),half2(1,1),half2(2,1),
        half2(-2,2),half2(-1,2),half2(0,2),half2(1,2),half2(2,2)
    };

    for(int it = 0;it < 25;it++)
    {
        boxuv[it] = uv + BOX_25[it]*unit.xy;
    }
}

//=====================================================================//

//=====================================================================//
//边缘检测
//Sobel
half VES_Sobel(float2 boxuv[9],sampler2D _Tex)
{
    const half VES_SOBEL_Gx[9] = 
    {
        -1,-2,-1,
        0, 0, 0,
        1, 2, 1
    };

    const half VES_SOBEL_Gy[9] = 
    {
        -1, 0, 1,
        -2, 0, 2,
        -1, 0, 1
    };

    half texColor;
    half eX = 0;
    half eY = 0;
    for(int n = 0;n<9;n++)
    {
        texColor = Luminance(tex2D(_Tex,boxuv[n]));
        eX += texColor * VES_SOBEL_Gx[n];
        eY += texColor * VES_SOBEL_Gy[n];
    }
    return 1 - (abs(eX) + abs(eY));
}
//=====================================================================//
//=====================================================================//
//Kernal
fixed4 VES_Kernal(half _Kernal[9],float2 _Boxuv[9],float _Factor,sampler2D _Tex)
{
    fixed4 color = fixed4(0,0,0,0);
    for(int n = 0;n<9;n++)
    {
        color += tex2D(_Tex,_Boxuv[n]) * _Kernal[n];
    }
    return color/_Factor;
}

fixed4 VES_Kernal_EdgeDetection01(float2 _Boxuv[9],sampler2D _Tex)
{
    const half VES_EdgeDetection_Matrix01[9] = 
    {
        1,0,-1,
        0,0,0,
        -1,0,1
    };
    return VES_Kernal(VES_EdgeDetection_Matrix01,_Boxuv,1,_Tex);
}

fixed4 VES_Kernal_EdgeDetection02(float2 _Boxuv[9],sampler2D _Tex)
{
    const half VES_EdgeDetection_Matrix02[9] = 
    {
        0,-1,0,
        -1,4,-1,
        0,-1,0
    };
    return VES_Kernal(VES_EdgeDetection_Matrix02,_Boxuv,1,_Tex);
}

fixed4 VES_Kernal_EdgeDetection03(float2 _Boxuv[9],sampler2D _Tex)
{
    const half VES_EdgeDetection_Matrix03[9] = 
    {
        -1,-1,-1,
        -1,8,-1,
        -1,-1,-1
    };
    return VES_Kernal(VES_EdgeDetection_Matrix03,_Boxuv,1,_Tex);
}

fixed4 VES_Kernal_Sharpen(float2 _Boxuv[9],sampler2D _Tex)
{
    const half VES_Sharpen_Matrix[9] = 
    {
        0,-1,0,
        -1,5,-1,
        0,-1,0
    };
    return VES_Kernal(VES_Sharpen_Matrix,_Boxuv,1,_Tex);
}

fixed4 VES_Kernal_Blur(float2 _Boxuv[9],sampler2D _Tex)
{
    const half VES_Blur_Matrix[9] = 
    {
        1,1,1,
        1,1,1,
        1,1,1
    };
    return VES_Kernal(VES_Blur_Matrix,_Boxuv,9,_Tex);
}

fixed4 VES_Kernal_GaussianBlur(float2 _Boxuv[25],sampler2D _Tex)
{
    const half VES_GaussianBlur_Matrix[25] = 
    {
        1, 4, 6, 4,1,
        4,16,24,16,4,
        6,24,36,24,6,
        4,16,24,16,4,
        1, 4, 6, 4,1,
    };
    
    fixed4 color = fixed4(0,0,0,0);
    for(int n = 0;n<25;n++)
    {
        color += tex2D(_Tex,_Boxuv[n]) * VES_GaussianBlur_Matrix[n];
    }
    return color/256;
}


fixed4 VES_Kernal_UnsharpMasking(float2 _Boxuv[25],sampler2D _Tex)
{
    const half VES_UnsharpMasking_Matrix[25] = 
    {
        1, 4, 6, 4,1,
        4,16,24,16,4,
        6,24,-476,24,6,
        4,16,24,16,4,
        1, 4, 6, 4,1,
    };
    
    fixed4 color = fixed4(0,0,0,0);
    for(int n = 0;n<25;n++)
    {
        color += tex2D(_Tex,_Boxuv[n]) * VES_UnsharpMasking_Matrix[n];
    }
    return color/-256;
}



//=====================================================================//

#endif  //VE_SHADER_UTILS
