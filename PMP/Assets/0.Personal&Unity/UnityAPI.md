# Unity不常见API全收集

### RectTransform.GetWorldCorners

获取矩形的四个顶点世界坐标，从左下角顺时针。

```C#
readonly Vector3[] m_WorldCorners = new Vector3[4];
t.GetWorldCorners(m_WorldCorners);
```

### Transform.InverseTransformPoint

将世界坐标position转换为本地坐标

```C#
readonly Vector3[] m_CanvasCorners = new Vector3[4];
m_CanvasCorners[i] = canvasTransform.InverseTransformPoint(m_WorldCorners[i]);
```

### Transform.TransformPoint

本地坐标转换为世界坐标



