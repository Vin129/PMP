using UnityEngine;
using UnityEngine.UI;

public class BezierMeshImage : Image 
{
    //控制点坐标
    Vector2 controlV2 = new Vector2(0,200);
    //密集度 mesh数量
    int intensity = 50;
    //mesh 宽度 
    int width = 8;
    // 以上属于动态配置参数,为了方便共享才放在此中，建议使用时提取出去动态配置。

    float halfW;
    Vector3[] bezierPoints;
    protected override void Start()
    {
        base.Start();
        halfW = width/2;
    }
    public void UpdateBezierPoints(Vector3 start, Vector3 end, Vector3 control, int segmentNum)
    {
        bezierPoints = Bezier.GetPointList(start, end, control, segmentNum);
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

        var start = new Vector3(v.x,v.y + r.height/2,1);
        var end = new Vector3(v.z,v.y + r.height/2,1);
        var c = new Vector3(controlV2.x,controlV2.y);
        UpdateBezierPoints(start,end,c,intensity);

        vh.Clear();
        for (int index=0; index < bezierPoints.Length; index++)
        {
            Vector3 baseVector = bezierPoints[index];
            var i = index*4;
            vh.AddVert(baseVector+new Vector3(-halfW,-halfW,1), color, new Vector2(0f, 0f));
            vh.AddVert(baseVector+new Vector3(-halfW,halfW,1), color, new Vector2(0f, 1f));
            vh.AddVert(baseVector+new Vector3(halfW,halfW,1), color, new Vector2(1f, 1f));
            vh.AddVert(baseVector+new Vector3(halfW,-halfW,1), color, new Vector2(1f, 0f));
            vh.AddTriangle(i,i+1,i+2);
            vh.AddTriangle(i+2, i+ 3,i);
        }
    }
}

public static class Bezier
{
    private static Vector3 CalculatePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        var p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
    public static Vector3[] GetPointList(Vector3 startPoint, Vector3 endPoint,Vector3 cPoint,int segmentNum)
    {
        Vector3[] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculatePoint(t, startPoint,cPoint,endPoint);
            path[i - 1] = pixel;
        }
        return path;
    }
}
