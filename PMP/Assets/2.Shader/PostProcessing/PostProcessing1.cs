using UnityEngine;
using UnityEngine.EventSystems;

//2D 扩张效果
[ExecuteInEditMode]
public class PostProcessing1 : MonoBehaviour,IPointerClickHandler
{
    public Material Effect;
    float mSlider = 0f;

    private void Update() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            var v3 = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Effect.SetVector("_Point",v3);
            mSlider = 0f;
            // Debug.LogError(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {
        if(Effect == null)
        {
            Graphics.Blit(src,dest);
            return;
        }
        if(mSlider < 2)
        {
            mSlider += Time.deltaTime;
            Effect.SetFloat("_Slider",mSlider);
            Graphics.Blit(src,dest,Effect);
            return;
        }
        Graphics.Blit(src,dest);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        // Camera.main.WorldToScreenPoint(eventData.position);
        // eventData.position
    }
}
