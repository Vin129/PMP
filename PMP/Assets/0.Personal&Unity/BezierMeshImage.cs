using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CurveMeshHelper))]
public class BezierMeshImage : Image { 
    CurveMeshHelper cmh;
    Vector3[] bezierPoints;
    int width = 4;
    public int num = 1000;
    public int Width {
        get
        {
            return width;
        }
        set
        {
            width = value;
        }
    }

    protected override void Start()
    {
        base.Start();
        cmh = GetComponent<CurveMeshHelper>();
    }

    void Update()
    {
       
    }

    public void SetBezierInfo(Vector3 startPos, Vector3 endPos, Vector3 controlPos, int segmentNum)
    {
        Vector3 startPoint = startPos;
        Vector3 endPoint = endPos;
        Vector3 controlPoint = controlPos;
        bezierPoints = UIHelper.GetBeizerList(startPoint, controlPoint, endPoint, segmentNum);
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if(cmh == null)
            cmh = GetComponent<CurveMeshHelper>();
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
        SetBezierInfo(new Vector3(v.x,v.y + r.height/2,1),new Vector3(v.z,v.y + r.height/2,1),new Vector3(cmh.C.x,cmh.C.y),cmh.Num);
        vh.Clear();
        for (int index=0; index < bezierPoints.Length; index++)
        {
            Vector3 baseVector = bezierPoints[index];
            var i = index*4;
            vh.AddVert(baseVector+new Vector3(-width,-width,1), color, new Vector2(0f, 0f));
            vh.AddVert(baseVector+new Vector3(-width,width,1), color, new Vector2(0f, 1f));
            vh.AddVert(baseVector+new Vector3(width,width,1), color, new Vector2(1f, 1f));
            vh.AddVert(baseVector+new Vector3(width,-width,1), color, new Vector2(1f, 0f));
            vh.AddTriangle(i,i+1,i+2);
            vh.AddTriangle(i+2, i+ 3,i);
        }
    }

}
