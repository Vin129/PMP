using UnityEngine;
using UnityEngine.EventSystems;

//3D 扩张效果
[ExecuteInEditMode]
public class PostProcessing2 : MonoBehaviour
{
    public bool Anim = false;
    public float Speed = 1f;   
    public Shader ReplacemenShader;
    [Range(0,1)]
    public float Slider = 0;
    [Range(0,1)]
    public float DepthParam = 1;
    [Range(0,1)]
    public float SliderWidth = 1;
    float mSlider = 0f;
    bool bTransition = false;
    Camera mCamera;

    private void Update() 
    {
        if(ReplacemenShader != null)
        {
            Shader.SetGlobalFloat("_Slider",Slider);
            Shader.SetGlobalFloat("_DepthParam",DepthParam);
            Shader.SetGlobalFloat("_SliderWidth",SliderWidth);
        }
        if(bTransition&& mSlider <= 2)
        {
            mSlider += Time.deltaTime * Speed;
            SliderWidth = mSlider;
        }
        else
        {
            bTransition = false;
        }
        if(Input.GetMouseButtonDown(0) && Anim)
        {
            var ray = mCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if(Physics.Raycast(ray,out hitInfo))
            {
                var v3 = mCamera.WorldToViewportPoint(hitInfo.point);
                float depth = v3.z/mCamera.farClipPlane;
                mSlider = 0f;
                Slider = depth;
                bTransition = true;
                Debug.LogError(depth);
            }
        }
    }
    
    private void OnEnable() 
    {
        mCamera = GetComponent<Camera>();
        if(ReplacemenShader != null)
            GetComponent<Camera>().SetReplacementShader(ReplacemenShader,"");    
    }

    private void OnDisable() {
        GetComponent<Camera>().ResetReplacementShader();
    }

    // private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    // {
    //     if(Effect == null)
    //     {
    //         Graphics.Blit(src,dest);
    //         return;
    //     }
    //     Graphics.Blit(src,dest,Effect);
    // }
}
