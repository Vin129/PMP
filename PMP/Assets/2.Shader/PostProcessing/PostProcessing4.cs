using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class PostProcessing4 : MonoBehaviour
{
    public Material Effect;
    public Shader ReplacemenShader;
    public Color EdgeColor = Color.black;
    Camera mCamera;

    private void Update() 
    {
        if(Effect != null)
        {
            Effect.SetColor("_EdgeColor",EdgeColor);
        }
        if(ReplacemenShader != null)
        {
            Shader.SetGlobalColor("_EdgeColor",EdgeColor);
        }
    }
    
    private void OnEnable() 
    {
        mCamera = GetComponent<Camera>();
        if(ReplacemenShader != null)
            GetComponent<Camera>().SetReplacementShader(ReplacemenShader,"XRay");    
    }

    private void OnDisable() {
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
