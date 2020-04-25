# Unity不常见API全收集

### RectTransform.GetWorldCorners

获取矩形的四个顶点世界坐标，从左下角顺时针。

```C#
readonly Vector3[] m_WorldCorners = new Vector3[4];
t.GetWorldCorners(m_WorldCorners);
```

***

### Transform.InverseTransformPoint

将世界坐标position转换为本地坐标

```C#
readonly Vector3[] m_CanvasCorners = new Vector3[4];
m_CanvasCorners[i] = canvasTransform.InverseTransformPoint(m_WorldCorners[i]);
```

### Transform.TransformPoint

本地坐标转换为世界坐标

***

### Rect.Overlaps

是否该矩形与other矩形有重叠

public bool **Overlaps**(Rect **other**);

public bool **Overlaps**(Rect **other**, bool **allowInverse**);

**allowInverse**：允许长宽出现负数，在此情况下依然正常检测

***

### CanvasRenderer.EnableRectClipping

在canvasRendered上启用矩形裁剪，指定矩形外的部分将被裁剪（不渲染）

public void **EnableRectClipping**(Rect **rect**);

禁用此CanvasRenderer的矩形裁剪。

public void **DisableRectClipping**();

