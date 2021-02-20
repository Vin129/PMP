using UnityEngine;
using UnityEngine.EventSystems;

//边缘检测
//基于图像的边缘检测
[ExecuteInEditMode]
public class PostProcessing3 : MonoBehaviour
{
    public Material Effect;
    public Shader ReplacemenShader;
    [Range(0,1)]
    public float Slider = 0;
    [Range(0,1)]
    public float DepthSlider = 0;
    [Range(0,1)]
    public float NdotVSlider = 0;
    public Color EdgeColor = Color.black;
    public Color BackgroundColor = Color.white; 
    Camera mCamera;

    private void Update() 
    {
        if(Effect != null)
        {
            Effect.SetFloat("_EdgeSlider",Slider);
            Effect.SetColor("_EdgeColor",EdgeColor);
            Effect.SetColor("_BackgroundColor",BackgroundColor);
            Effect.SetFloat("_DepthThreshold",DepthSlider);
            Effect.SetFloat("_NdotVThreshold",NdotVSlider);
        }
        if(ReplacemenShader != null)
        {
            Shader.SetGlobalColor("_EdgeColor",EdgeColor);
            Shader.SetGlobalColor("_BackgroundColor",BackgroundColor);
            Shader.SetGlobalFloat("_NdotVThreshold",NdotVSlider);
            Shader.SetGlobalFloat("_EdgeSlider",Slider);
        }
    }
    
    private void OnEnable() 
    {
        mCamera = GetComponent<Camera>();
        mCamera.depthTextureMode = DepthTextureMode.DepthNormals;
        if(ReplacemenShader != null)
            GetComponent<Camera>().SetReplacementShader(ReplacemenShader,"");    
    }

    private void OnDisable() {
        mCamera.depthTextureMode = DepthTextureMode.None;
        GetComponent<Camera>().ResetReplacementShader();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {
        if(Effect == null)
        {
            Graphics.Blit(src,dest);
            return;
        }
        Graphics.Blit(src,dest,Effect);
    }
}
