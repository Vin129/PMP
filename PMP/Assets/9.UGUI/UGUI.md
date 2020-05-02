# UGUI

如果你在正在使用Unity，那你很难不去接触它的UI部分。

本专栏将针对Unity的UGUI源码对整个UGUI进行系统的讲解。

通过现象看本质，理解掌握，使你更加得心应手。

# Base

## UIBehaviour

> **BaseClass: MonoBehaviour**
>
> **Interface: 无**
>
> **Intro: UGUI组件的基础类，为UGUI组件提供了三个模块通用接口。**

- **MonoBehaviour 生命周期**
- **UnityEditor 辅助方法**
- **UGUI 通用方法**



***

## EventSystem

> **Related Class: BaseInputModule、BaseEventData、 ExecuteEvents、RaycasterManager+**
>
> **Related  Interface: 无**
>
> **Related Other: 无**
>
> **Intro: UGUI事件管理系统，通过此系统完成我们所知的UI交互。**



**基本使用：GameObject上挂有IEventSystemHandler组件并是可用状态**

**EventSystem:  **

​	**管理 所有的输入检测模块（InputModule）并帧调用Module的执行（Process）**

​	**调动射线捕捉模块（Raycasters），为InputModule提供结果（具体的触点所穿透的对象信息）**

​	**InputModule 管理更新EventData 判断当前的操作事件，并通知具体的EventSystemHandler 进行逻辑处理**

***

### EventData

**主要分三类来存储事件信息，对应具体的EventSystemHandler** 

- BaseEventData：基础的事件信息
- PointerEventData: 存储 触摸/点击/鼠标操作 事件信息
- AxisEventData：移动相关的事件信息，注：拖动的操作属于PointerEventData



***

### **BaseInputModule **

**被EventSystem帧调用，检查Input中各项数值 => 判断当前操作状态，更新/创建 PointerEventData并以当前操作状态进行组装 =>  Mouse : buttonData   Touch : m_PointerData**

**主要分为单点触控（StandaloneInputModule）与多点触控（~~TouchInputModule~~ Touch模块已经整合进Standalone中）模块。**

- **ProcessMouseEvent：处理所有鼠标事件**
- **ProcessTouchEvents：处理所有触点事件**

#### **ProcessMouseEvent**

- MouseState ：获取当前鼠标状态即鼠标左键、右键、中键的状态（ButtonState）
- ProcessMousePress、ProcessMove、ProcessDrag 检测各个ButtonState下的PointerEventData
- 满足条件则执行相应的事件（ExecuteEvents）

#### ProcessTouchEvents

- ProcessTouchPress、ProcessMove、ProcessDrag 检测TouchId下的PointerEventData
- 满足条件则执行相应的事件（ExecuteEvents）

***

### Raycasters

**在事件系统中充当捕获物体的角色，管理射线，为InputModule提供gameObject**

#### **RaycasterManager**

**管理了一个 RaycasterList，通过EventSystem.RaycastAll提供给InputModule**

- Raycaster启用时（Enable）加入List
- Raycaster弃用时（Disable）移除List

#### Physics2DRaycaster PhysicsRaycaster

由Manager触发射线（**Raycast**），返回射线结果（**RaycastResult**）

2D与3D分别使用了Physics2D/Physics 中射线穿透获取交点信息的方法

**Raycast的过程**

- `eventSystem.RaycastAll(pointerData, m_RaycastResultCache);InputModule检测到了点击\触摸，并向EventSystem请求数据` 

-  `module.Raycast(eventData, raycastResults); EventSystem响应InputModule的请求,启动射线获取数据`
- `ray = eventCamera.ScreenPointToRay(eventData.position); 通过位置信息获取射线`
- `m_Hits = ReflectionMethodsCache.Singleton.raycast3DAll(ray, ...);通过射线获取最近的距离数据hitDistance`
- `Raycast(canvas, currentEventCamera, eventPosition, canvasGraphics, m_RaycastResults);遍历所有Graphic,获取可接收射线广播的Graphic信息`
- `var result = new RaycastResult{....};resultAppendList.Add(result);将所有数据进行封装提供给EventSystem`

```C#
var castResult = new RaycastResult
{
    gameObject = go,//接收射线广播的物体
    module = this,//射线采集器Raycaster
    distance = distance,//最近距离
    screenPosition = eventPosition,//事件坐标(鼠标\触点坐标)
    index = resultAppendList.Count,//索引
    depth = m_RaycastResults[index].depth,//Graphic深度
    sortingLayer = canvas.sortingLayerID,
    sortingOrder = canvas.sortingOrder
};
```





#### RaycastResult

射线数据封装结构，包含了有关射线经过物体的具体信息



***

### EventSystemHandler

事件处理者，这里会提供许多事件接口。最终事件响应的具体逻辑就由我们继承这些接口来书写。

主要分为三种类型

- IPointerXXXHandler ： 处理鼠标点击和触屏事件
- IDragXXXXHandler：处理拖拽事件
- IXXXHandler：处理其他如选择、取消等事件



***

### ExecuteEvents

事件执行器，提供了一些通用的处理方法。

Execute过程：

`GetEventList<T>(target, internalHandlers); 获取物体上所有包含IEventSystemHandler且可用的组件`

`arg = (T)internalHandlers[i]; 找到具体T类型组件 `

`functor(arg, eventData); 执行`

`handler.OnXXXX(ValidateEventData<XXXXEventData>(eventData)); functor 类似这样`



**ExecuteHierarchy**：逐节点寻找可执行的物体（含可用的IEventSystemHandler），触发（Execute）即停止逐根搜索，**return**



***

## CanvasUpdateSystem

> **Related Class:  Canvas、CanvasUpdateRegistry、ClipperRegistry**
>
> **Related  Interface: ICanvasElement**
>
> **Related Other: Enum CanvasUpdate**
>
> **Intro: 由Canvas控制，通过 ICanvasElement 接口，使用脏标记方法来统一更新CanvasElement**

**ICanvasElement**

- 重建方法：void Rebuild(CanvasUpdate executing);
- 布局重建完成：void LayoutComplete();
- 图像重建完成：void GraphicUpdateComplete();
- 检查Element是否无效：bool IsDestroyed();

**Registry 管理着两个队列**

1. LayoutRebuildQueue：布局重建
2. GraphicRebuildQueue：图像重建

CanvasUpdateRegistry 被初始化时（构造函数）向Canvas中注册了更新函数（PerformUpdate），以用来响应重建。

```C#
Canvas.willRenderCanvases += PerformUpdate;
```

**PerformUpdate**

Canvas在渲染前会调用willRenderCanvases，即执行PerformUpdate ，流程如下：

- 首先更新布局，根据父节点多少排序，由内向外更新。更新类型依次为 Prelayout 、Layout 、PostLayout（enum CanvasUpdate）
- 通知布局完成
- ClipperRegistry 进行剪裁  ([MaskableGraphic](###MaskableGraphic))
- 更新图像，依次 PreRender、LatePreRender、MaxUpdateValue
- 通知图像更新完成

**脏标**

标记延迟执行，优化重新渲染的手段。

例如在Graphic 中存在三种脏标分别代表三种等待重建

1. 尺寸改变时（RectTransformDimensions）：LayoutRebuild 布局重建
2. 尺寸、颜色改变时：Vertices to GraphicRebuild  图像重建
3. 材质改变时：Material to GraphicRebuild  图像重建

层级改变、应用动画属性（DidApplyAnimationProperties） ：All to Rebuild 重建所有



***

## Graphic

> **Related Class: Graphic、MaskableGraphic、GraphicRegistry、CanvasUpdateRegistry、VertexHelper**
>
> **Related  Interface: ICanvasElement、IMeshModifier、IClippable、IMaskable、IMaterialModifier**
>
> **Related Other:** 
>
> **Intro: 图形组件的基类，形成图像**

- **ICanvasElement**: Canvas元素(重建接口)，当Canvas发生更新时重建（void Rebuild）
- **IMeshModifier**：网格处理接口
- **IClippable**：裁剪相关处理接口
- **IMaskable**：遮罩处理接口
- **IMaterialModifier**：材质处理接口

**Graphic 作为图像组件的基类，主要实现了网格与图像的生成/刷新方法。**
**在生命周期Enable阶段、Editor模式下的OnValidate中、层级/颜色/材质改变时都会进行相应的刷新（重建）。**
**重建过程主要通过 CanvasUpdateSystem 最终被Canvas所重新渲染。**

**重建主要分为两个部分：顶点重建（UpdateGeometry）与 材质重建（UpdateMaterial）**

**更新完成的结果会设置进CanvasRenderer，从而被渲染形成图像。**

![](G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\maskableGraphic.png)

***

### GraphicRegistry

管理同Canvas下的所有Graphic对象

`Dictionary<Canvas, IndexedSet<Graphic>> m_Graphics`

Graphic 初始化时（Enable）会寻找其最近根节点的**Canvas**组件，并以此为key存储在**GraphicRegistry**中。



***

### UpdateGeometry

**Graphic 顶点（网格）更新与生成，发生顶点重建时会被调用。**

过程：

- 更新**VertexHelper**数据
- 遍历身上的**IMeshModifier**组件（MeshEffect组件，实现网格的一些特效，例如Shadow、Outline），更新**VertexHelper**数据
- 将最终的顶点数据设置给 **workerMesh**，并将**workerMesh**设置进**canvasRenderer**中，进行渲染。

```C#
private void DoMeshGeneration()
{
    if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
        OnPopulateMesh(s_VertexHelper);//更新顶点信息
    else
        s_VertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

    var components = ListPool<Component>.Get();
    GetComponents(typeof(IMeshModifier), components);

    for (var i = 0; i < components.Count; i++)
        ((IMeshModifier)components[i]).ModifyMesh(s_VertexHelper);//若由网格特效，则由特效继续更新顶点信息

    ListPool<Component>.Release(components);

    s_VertexHelper.FillMesh(workerMesh);
    canvasRenderer.SetMesh(workerMesh);//设置当canvasRenderer中
}
```



**基础的网格由 4 个顶点 2 个三角面构成**

<img src="G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\graphicMesh.png" style="zoom:67%;" />

**VertexHelper** ： 临时存储有关顶点的所有信息，辅助生成网格

> \- List\<Vector3> m_Positions : 顶点位置
>
> \- List\<Color32> m_Colors ：顶点颜色
>
> \- List\<Vector2> m_Uv0S ：第1个顶点UV坐标
>
> \- List\<Vector2> m_Uv1 ：第2个顶点UV坐标
>
> \- List\<Vector2> m_Uv2S ：第3个顶点UV坐标
>
> \- List\<Vector2> m_Uv3S ：第4个顶点UV坐标
>
> \- List\<Vector3> m_Normals ：法线向量
>
> \- List\<Vector4> m_Tangents ： 切线向量
>
> \- List\<int> m_Indices ： 三角面顶点索引

**BaseMeshEffect**

- ​	**PositionAsUV1**: 根据顶点坐标设置UV1坐标（一般为法线贴图，不加此组件时UV1坐标默认是**Vector2.zero**）
- ​	**Shadow**：在顶点数基础上增加了一倍的顶点数，并根据偏移（effectDistance）设置新顶点的坐标，实现阴影效果。
- ​	**Outline**：继承自Shadow，原理就是分别在四个角（根据effectDistance换算）上实现了四个Shadow，即增加了4倍的顶点数。

![](G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\MeshModifier.png)

***

### UpdateMaterial

**Graphic 材质更新，发生材质重建时会被调用。**

过程：

- 获取自身材质**material**，遍历身上的**IMaterialModifier**组件（材质处理组件，实现材质特效，例如Mask），更新 **materialForRendering**
- 将最终的材质数据**materialForRendering**与纹理**mainTexture**设置进**canvasRenderer**中，进行渲染。

```C#
protected virtual void UpdateMaterial()
{
    if (!IsActive())
        return;
    canvasRenderer.materialCount = 1;
    canvasRenderer.SetMaterial(materialForRendering, 0);
    canvasRenderer.SetTexture(mainTexture);
}

public virtual Material materialForRendering
{
    get
    {
        var components = ListPool<Component>.Get();
        GetComponents(typeof(IMaterialModifier), components);

        var currentMat = material;
        for (var i = 0; i < components.Count; i++)
            currentMat = (components[i] as IMaterialModifier).GetModifiedMaterial(currentMat);//这里由IMaterialModifier组件对currentMat进行特效化处理，得到最终展示的材质
        ListPool<Component>.Release(components);
        return currentMat;
    }
}
```

***

### MaskableGraphic

> **BaseClass: Graphic**
>
> **Interface: IClippable、IMaskable、IMaterialModifier**

#### 简介

**MaskableGraphic**在 **Graphic**的基础上实现了**裁剪与遮罩功能**。

这主要是由 **IClippable**、**IMaskable** 两个接口来实现的。

​	在Graphic更新材质的流程中有提及Mask。Graphic 可以理解成由骨头和皮肤所组成，骨头即顶点信息所构建的网格（Mesh），皮肤则是依附于Mesh的材质和纹理。实际上Mesh是不可见的，对于可见物的处理（例如Mask遮罩剔除）都是针对于Material。

**理解清楚IClippable与IMaskable相关的组件原理便是理解MaskableGraphic的关键**

**IClipper 矩形裁剪  与  IMaskable基于Material的遮罩**

[IClipper&IClippable](##RectMask2D)

[IMaskable](##Mask)

***

# Component

## RectMask2D

> **BaseClass: UIBehaviour**
>
> **Interface: IClipper、ICanvasRaycastFilter**
>
> **Intro: 这是UGUI提供的不依赖于Graphic的裁剪组件，它的原理在于设置IClippable组件中canvasRenderer.EnableRectClipping 来实现矩形裁剪效果**

**IClipper（裁剪者）与 IClippable（可裁剪对象)**

**RectMask2D的工作原理**：

- **RectMask2D**是**IClipper**，当启动时（Enable）先向**ClipperRegistry**中注册自己，然后会调用其所有子节点下**IClippable** 组件的**RecalculateClipping**方法，将其添加进最近父节点中的**RectMask2D**中（这是为了避免各种嵌套带来的浪费）

```C#
// MaskableGraphic 中更新裁剪者的方法
private void UpdateClipParent()
{
    var newParent = (maskable && IsActive()) ? MaskUtilities.GetRectMaskForClippable(this) : null;

    // if the new parent is different OR is now inactive
    if (m_ParentMask != null && (newParent != m_ParentMask || !newParent.IsActive()))
    {
        m_ParentMask.RemoveClippable(this);
        UpdateCull(false);
    }

    // don't re-add it if the newparent is inactive
    if (newParent != null && newParent.IsActive())
        newParent.AddClippable(this);

    m_ParentMask = newParent;
}
```

- 当**Canvas**进行刷新的时候（**[CanvasUpdateSystem](##CanvasUpdate System)**），会调用所有启用中的**IClipper**，执行**Cull 操作，遍历执行 IClipper.PerformClipping**

  `ClipperRegistry.instance.Cull();`

  `m_Clippers[i].PerformClipping();`

- **PerformClipping :** 目的在于更新**IClippable**中用于裁剪的**Rect**

  首先会借助**MaskUtilities、Clipping** 寻找的最小的裁剪框**clipRect**

  接着会遍历自身下所有的**IClippable**组件（由IClippable.RecalculateClipping 添加）设置clipRect

  ​	`clipTarget.SetClipRect(clipRect, validRect)  validRect:用于判断裁剪框是否可用（长宽>0）`

  ​	`canvasRenderer.EnableRectClipping(clipRect) MaskableGraphic 中设置裁剪框`

  最后会判断是否改变**IClippable**中cull的状态

  ​	`canvasRenderer.cull = cull;`

  在此流程期间**RectMask2D**会优化处理过程：

  ​	1.记录上次的**clipRect**来判断裁剪矩形是否发生变化，从而省略没必要的重新裁剪。

  ​		`m_LastClipRectCanvasSpace = clipRect;`

  ​	2.裁剪层的子集合会因为父级的裁剪而被裁剪，因此可以传递无效的rect来避免重复的处理。

  ​		        `clipTarget.Cull(maskIsCulled ? Rect.zero : clipRect,maskIsCulled ? false : validRect)`



***

## Mask

> **BaseClass: UIBehaviour**
>
> **Interface: IMaterialModifier、ICanvasRaycastFilter**
>
> **Intro: 这是UGUI提供的以纹理为模板的遮罩组件，它的原理在于以MaskableGraphic的基本材质上增加遮罩属性内容从而生成新的遮罩材质来达到遮罩的目的**

**MaskableGraphic 中IMaskable实现：**

- 当**Enable**时，若该物体自身含有**Mask组件**则会调用其子节点路径下所有**IMaskable组件方法**。

```C#
protected override void OnEnable()
{
    base.OnEnable();
    m_ShouldRecalculateStencil = true; //控制是否从新计算遮罩深度->改变遮罩材质
    UpdateClipParent();
    SetMaterialDirty();

    if (GetComponent<Mask>() != null)
    {
        // 设置Mask遮罩状态
        MaskUtilities.NotifyStencilStateChanged(this);
    }
}

//IMaskable 接口方法
public virtual void RecalculateMasking()
{
    m_ShouldRecalculateStencil = true;
    SetMaterialDirty();
}
```

- 在**Grahpic材质重建**的过程中会调用其上所有**IMaterialModifier组件方法**来处理最终的渲染材质**materialForRendering**

```C#
//MaskableGraphic 中IMaterialModifier 组件方法
public virtual Material GetModifiedMaterial(Material baseMaterial)
{
    var toUse = baseMaterial;//来自Graphic的基础材质
    if (m_ShouldRecalculateStencil)
    {
        var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
        m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
        m_ShouldRecalculateStencil = false;
    }
    // 优化了遮罩处理，如果已经启用了Mask组件，则不必再次做重复的事情
    Mask maskComponent = GetComponent<Mask>();
    if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive()))
    {
        //借助StencilMaterial生产一个新的遮罩材质，这里是使用list存储避免重复生成一样的材质
        var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
        StencilMaterial.Remove(m_MaskMaterial);
        m_MaskMaterial = maskMat;
        toUse = m_MaskMaterial;
    }
    return toUse;//返回新生成的遮罩材质
}
```





***

## RawImage

> **BaseClass: MaskableGraphic**
>
> **Interface: None**
>
> **Intro: 顾名思义，Raw，未加工的图片。**

**谈及RawImage，便会想到Image。两者都是继承自MaskableGraphic，但RawImage的实现相比较于Image（1200+行）简单了非常多。**

**RawImage**仅仅只重写了**Graphic**中的[OnPopulateMesh](###UpdateGeometry)，提供了一个**Rect变量**来使用户可以**自定义UV的坐标**。

```C#
protected override void OnPopulateMesh(VertexHelper vh)
{
    Texture tex = mainTexture;
    vh.Clear();
    if (tex != null)
    {
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
        var scaleX = tex.width * tex.texelSize.x;
        var scaleY = tex.height * tex.texelSize.y;
        {
            var color32 = color;
            //根据m_UVRect来设置UV坐标
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMin * scaleY));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(m_UVRect.xMin * scaleX, m_UVRect.yMax * scaleY));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMax * scaleY));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(m_UVRect.xMax * scaleX, m_UVRect.yMin * scaleY));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
    }
}
```



***

## Image

> **BaseClass: MaskableGraphic**
>
> **Interface: ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter**
>
> **Intro: UGUI中最重要的组件， 它最核心的用途便是“显示图片”，并且提供了四种类型来丰富其显示图片的功能（ImageType:Simple,Sliced,Tiled,Filled）**

- **ISerializationCallbackReceiver**：提供了序列化前和反序列化后的处理接口
- **ILayoutElement**：布局元素，可以被布局组件(ILayoutController)布局
- **ICanvasRaycastFilter**：判断射线是否可以有效作用与该Image

**初始化：**

Image在Enable与Disable时会添加或取消对图片的跟踪

`TrackImage(this);` `UnTrackImage(this);`

向SpriteAtlasManager中注册重建方法。这一段是专门为SpriteAtlas准备的。



**Image的代码量固然多（1200+），乍看下来便心生敬畏。所以作为Graphic组件，首先别先从它的Mesh与Material处理相关分析。**

### Mesh

**Image 不同于RawImage的最重要的一点在于Image使用了Sprite取代Texture作为存储图像信息的类型,Sprite是Unity专门为2D游戏(通常也用于3D游戏的UI)提供的图像对象。它记录了有关图形的许多信息（texture,uv,triangles,vertices...），通过SpriteRenderer来显示图形。**

```C#
//1200+行的Image,一大半都是来自于这的实现。
protected override void OnPopulateMesh(VertexHelper toFill)
{
    if (activeSprite == null)
    {
        base.OnPopulateMesh(toFill);
        return;
    }
	//根据不同的类型进行相应的处理
    switch (type)
    {
        case Type.Simple:
            GenerateSimpleSprite(toFill, m_PreserveAspect);
            break;
        case Type.Sliced:
            GenerateSlicedSprite(toFill);
            break;
        case Type.Tiled:
            GenerateTiledSprite(toFill);
            break;
        case Type.Filled:
            GenerateFilledSprite(toFill, m_PreserveAspect);
            break;
    }
}
```

- **Type.Simple：**简单的通过顶点颜色UV生成Mesh,顶点与三角面数量保持不变

- **Type.Sliced:**   当Sprite被编辑(Sprite Editor)，使其边界值(activeSprite.border)>0时可以使用该类型。

  根据Sprite编辑后的信息，至多可划分9块矩形区域，处在边界区域内的图像会被拉伸，边界区域外的图像保持不变。

- **Type.Tiled：**计算边界区域内的图像大小，按照此大小平铺满整个矩形。矩形区域数量取决于平铺数量。

- **Type.Filled:  ** 主要通过fillAmount(填充率)来对矩形进行不同类型的填充（水平、垂直、角度）



***

### Material

在材质纹理的处理上，仅仅是多了对带有Alpha通道的图片的渲染操作(这个取决于原图片是否采取含有透明通道的压缩方式，若不含有透明通道，则为null)

```C#
protected override void UpdateMaterial()
{
    base.UpdateMaterial();

    // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

    if (activeSprite == null)
    {
        canvasRenderer.SetAlphaTexture(null);
        return;
    }

    Texture2D alphaTex = activeSprite.associatedAlphaSplitTexture;

    if (alphaTex != null)
    {
        canvasRenderer.SetAlphaTexture(alphaTex);
    }
}
```



***

### Raycast

Image的射线相关接口，主要通过判断坐标点的像素透明度是否满足接收射线的透明度值来判断是否可以有效响应射线。

**alphaHitTestMinimumThreshold：** 是可以被动态设置的值，默认值为0，通过该值控制射线响应条件。

```C#
public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
{
    if (alphaHitTestMinimumThreshold <= 0)
        return true;
    if (alphaHitTestMinimumThreshold > 1)
        return false;
    if (activeSprite == null)
        return true;
    //计算作用位置坐标
    Vector2 local;
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local))
        return false;
    Rect rect = GetPixelAdjustedRect();
    // Convert to have lower left corner as reference point.
    local.x += rectTransform.pivot.x * rect.width;
    local.y += rectTransform.pivot.y * rect.height;
    local = MapCoordinate(local, rect);
    // Normalize local coordinates.
    Rect spriteRect = activeSprite.textureRect;
    Vector2 normalized = new Vector2(local.x / spriteRect.width, local.y / spriteRect.height);
    // Convert to texture space.
    float x = Mathf.Lerp(spriteRect.x, spriteRect.xMax, normalized.x) / activeSprite.texture.width;
    float y = Mathf.Lerp(spriteRect.y, spriteRect.yMax, normalized.y) / activeSprite.texture.height;
    try
    {
        //判断坐标位置的像素透明度是否满足射线接收的最小透明度
        return activeSprite.texture.GetPixelBilinear(x, y).a >= alphaHitTestMinimumThreshold;
    }
    catch (UnityException e)
    {
        Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this);
        return true;
    }
}
```



***








# 资料链接

[processon](https://www.processon.com/diagraming/5e8953e5e4b0bf3ebcf8be7d)

[源码地址](https://bitbucket.org/Unity-Technologies/ui/src/2017.4/)

[UGUI使用教程](https://gameinstitute.qq.com/community/search?keyword=UGUI使用教程)

# 用时

**16h**