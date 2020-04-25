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

-  `module.Raycast(eventData, raycastResults); EventSystem启动射线`

- `ray = eventCamera.ScreenPointToRay(eventData.position); 通过位置信息获取射线`
- `m_Hits = ReflectionMethodsCache.Singleton.raycast3DAll(ray, ...);通过射线获取穿透数据`
- `var result = new RaycastResult{....};resultAppendList.Add(result);将穿透数据进行封装`

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
> **Related  Interface: ICanvasElement、IMeshModifier**
>
> **Related Other: **
>
> **Intro: 图形组件的基类，形成图像**

- **ICanvasElement**: Canvas元素(重建接口)，当Canvas发生更新时重建（void Rebuild）
- **IMeshModifier**：网格处理接口

**Graphic 作为图像组件的基类，主要为具体的图形组件提供了图像生成与刷新方法。**

**通过 CanvasUpdateSystem 而被Canvas命令重建（渲染)。**

**重建主要分为两个部分：顶点重建（UpdateGeometry）与 材质重建（UpdateMaterial）**

**更新完成的结果会设置进CanvasRenderer，从而被渲染形成图像。**

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



***

### UpdateMaterial

**Graphic 材质更新，发生材质重建时会被调用。**

过程：

- 获取自身材质**material**，遍历身上的**IMaterialModifier**组件（材质处理组件，实现材质特效，例如Mask），更新**VertexHelper**数据
- 将最终的材质数据**materialForRendering**与纹理**mainTexture**设置进**canvasRenderer**中，进行渲染。



***

### MaskableGraphic

> **BaseClass: Graphic**
>
> **Interface: IClippable、IMaskable、IMaterialModifier**
>
> **Intro: 继承Graphic，是，在此基础上实现了剔除、遮罩功能**



**这里需要先理解Mask相关的组件原理以便更好的了解MaskableGraphic**

​	在Graphic更新材质的流程中有提及Mask。Graphic 可以理解成由骨头和皮肤所组成，骨头即顶点信息所构建的网格（Mesh），皮肤则是依附于Mesh的材质和纹理。实际上Mesh是不可见的，对于可见物的处理（例如Mask遮罩剔除）都是针对于Material。

**Mask 主要分成两个部分：IClipper 矩形裁剪  与  IMaskable基于Material的遮罩**

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
















# 资料链接

[processon](https://www.processon.com/diagraming/5e8953e5e4b0bf3ebcf8be7d)

[源码地址](https://bitbucket.org/Unity-Technologies/ui/src/2017.4/)

[UGUI使用教程](https://gameinstitute.qq.com/community/search?keyword=UGUI使用教程)

# 用时

**13h**