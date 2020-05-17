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

****

### Execute XXXHandler

对这个写法是不是很熟悉呢？作者之前的文章中也经常这么来提及（**EventSystem中的处理接口**）。那其实在之前**EventSystem**章节中已经分析了事件系统的执行的整体流程了，今天我们随着Button组件来深入地来分析具体的**Handler**是如何被**检查并触发**的。

**STEP1.一切都是由EventSystem的Update开始的：**

```C#
//EventSystem
protected virtual void Update()
{
    if (current != this)
        return;
    TickModules();//遍历并刷新所有的InputModules,更新Modules中的m_LastMousePosition、m_MousePosition           
    //省略中间遍历检查是否需要变更当前m_CurrentInputModule的部分
  	.....
  	//执行当前InputModule的Process,由此开始判断事件
    if (!changedModule && m_CurrentInputModule != null)
        m_CurrentInputModule.Process();
}
```

**STEP2.InputModule先会进行对外设输入的检测，来更新导航或是确定操作。紧接着会开始触摸检测，若不存在触摸，则进行鼠标事件的检测，因为触摸事件的检测是鼠标检测的简化版，所有下面我们针对鼠标检测进行分析。**

```C#
//StandaloneInputModule
public override void Process()
{
    if (!eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
        return;
    //向当前选中的目标执行UpdateSelectedHandler,并返回是否执行了
    bool usedEvent = SendUpdateEventToSelectedObject();

    //若用导航的情况会执行MoveHandler与SubmitHandler,当执行成功某一项时停止
    if (eventSystem.sendNavigationEvents)
    {
        if (!usedEvent)
            usedEvent |= SendMoveEventToSelectedObject();
        if (!usedEvent)
            SendSubmitEventToSelectedObject();
    }
    //以上部分是用来检测键盘输入的部分，例如使用键盘方向键选择按钮、使用ENTER键执行Submit。
    //接着开始先进行触摸的事件检测，如果不存在触摸，则会进行鼠标的事件检测
    if (!ProcessTouchEvents() && input.mousePresent)
        ProcessMouseEvent();
}
```

**STEP3.鼠标事件的检测过程，从中我们也能很清楚的了解到各个Handler的执行顺序**（方法的复杂度在不断提升）

```C#
//StandaloneInputModule
protected void ProcessMouseEvent(int id)
{
    //实际上这个id也没有用，每次都是获取左中右三个按键的信息
    //这里包含了PointerEventData数据与ButtonState数据，前者主要记录事件相关的信息，后者记录鼠标按键的当前状态
    var mouseData = GetMousePointerEventData(id);
    var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;
    m_CurrentFocusedGameObject = leftButtonData.buttonData.pointerCurrentRaycast.gameObject;
	//执行鼠标按压的过程(根据buttonState来判断并执行 PointerDown PointerUp PointerClick Drop EndDrag 事件)
    ProcessMousePress(leftButtonData);
    //执行鼠标移动过程(根据pointerEvent判断并执行 PointerEnter PointerExit 事件)
    ProcessMove(leftButtonData.buttonData);
    //执行拖拽过程(根据pointerEvent判断并执行 BeginDrag Drag PointerUp 事件)
    ProcessDrag(leftButtonData.buttonData);
	
    //以下是对鼠标右键与中键的相同执行
 ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
  ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
  ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
  ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

    //检测是否存在滚动事件，参数来自于input.mouseScrollDelta，这里使用了leftButtonData,实际上scrollDelta三个ButtonData里都是一样的，因为在GetMousePointerEventData方法中其他两个buttonData都是Copy leftData:)
    if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
    {
        var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
        ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
    }
}
```

**STEP4.深入其中，点击事件的判断。**

- **在按下的情况下：PointerDown会先被执行，其次会检查物体是否有DragHandler，如果存在则会执行InitializePotentialDrag，这个会在发生拖拽之前执行。**
- **在抬起的情况下（完成的点击操作）：先会执行PointerUp，其次执行PointerClick，接着时Drop，最后时EndDrag。**

```C#
//StandaloneInputModule
protected void ProcessMousePress(MouseButtonEventData data)
{
    var pointerEvent = data.buttonData;
    var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;
    //判断当前是否是按下状态(都可以包含按下和抬起同帧情况)
    if (data.PressedThisFrame())
    {
        pointerEvent.eligibleForClick = true;
        pointerEvent.delta = Vector2.zero;
        pointerEvent.dragging = false;
        pointerEvent.useDragThreshold = true;
        pointerEvent.pressPosition = pointerEvent.position;
        pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

        DeselectIfSelectionChanged(currentOverGo, pointerEvent);
        
        //搜索父级路径下是否有IPointerDownHandler组件并执行
        var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

        //如果自身及父级路径下没有IPointerDownHandler，则检查该路径下的IPointerClickHandler。
        if (newPressed == null)
            newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

        float time = Time.unscaledTime;
        //若按压物体为发生变化，更新点击信息
        if (newPressed == pointerEvent.lastPress)
        {
            var diffTime = time - pointerEvent.clickTime;
            if (diffTime < 0.3f)
                ++pointerEvent.clickCount;
            else
                pointerEvent.clickCount = 1;
            pointerEvent.clickTime = time;
        }
        else
        {
            pointerEvent.clickCount = 1;
        }
        pointerEvent.pointerPress = newPressed;
        pointerEvent.rawPointerPress = currentOverGo;
        pointerEvent.clickTime = time;

        pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);
        //当存在IDragHandler时，先触发 initializePotentialDrag 事件 这个事件在BeginDrag之前
        if (pointerEvent.pointerDrag != null)
            ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);

        m_InputPointerEvent = pointerEvent;
    }

    //当抬起时(都可以包含按下和抬起同帧情况)
    if (data.ReleasedThisFrame())
    {
        // 最先执行PointerUp事件
        ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

        var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

        if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
        {
            // 其次才执行PointerClick事件
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
        }
        else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
        {
            // Drop 事件会在 EndDrag 之前执行
            ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
        }

        pointerEvent.eligibleForClick = false;
        pointerEvent.pointerPress = null;
        pointerEvent.rawPointerPress = null;

        // 最后执行EndDrag 事件
        if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

        pointerEvent.dragging = false;
        pointerEvent.pointerDrag = null;

        if (currentOverGo != pointerEvent.pointerEnter)
        {
            HandlePointerExitAndEnter(pointerEvent, null);
            HandlePointerExitAndEnter(pointerEvent, currentOverGo);
        }

        m_InputPointerEvent = pointerEvent;
    }
}
```

```C#
//PointerInputModule  检查鼠标按键状态
protected PointerEventData.FramePressState StateForMouseButton(int buttonId)
{
    var pressed = input.GetMouseButtonDown(buttonId); //按下
    var released = input.GetMouseButtonUp(buttonId); //抬起
    if (pressed && released)
        return PointerEventData.FramePressState.PressedAndReleased;
    if (pressed)
        return PointerEventData.FramePressState.Pressed;
    if (released)
        return PointerEventData.FramePressState.Released;
    return PointerEventData.FramePressState.NotChanged;//无变化
}
```

**到此位置，输入事件的检测与执行流程已经分析完毕了。当我们对EventSystem深入理解之后，我们便可以更好的去使用交互组件，并根据自己的需求修改扩展它们。**









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

## Selectable

> **BaseClass: UIBehaviour**
>
> **Interface: IMoveHandler、IPointerDownHandler、IPointerUpHandler、IPointerEnterHandler、IPointerExitHandler、ISelectHandler、IDeselectHandler**
>
> **Intro: UGUI事件响应组件(Button、Toggle...)的基类，UGUI重要的组成部分，是EventSystem的具体的接收方。**

- **IMoveHandler**: 接收通过外设(键盘、手柄等)的方向键输入的响应接口
- **IPointerXXXHandler**：点击\触摸输入的响应接口
- **ISelectHandler**：当该物体被选中时的响应接口，取决于**EventSystem**的**m_CurrentSelected**
- **IDeselectHandler**：当该物体取消选中时的响应接口，取决于**EventSystem**的**m_CurrentSelected**

**Selectable**，输入事件都是基于对象而被响应的(例如：点击按钮，拖拽物体)必须有物体，而可以触发事件的物体的基本条件便是**“可选中“**，以此为基类可以满足广大的操作受众(例如手柄操作没法像鼠标那样可以直击按钮，需要通过方向键选中按钮才可以进行点击操作)。

**SelectHandler Selectable的基础事件**：由输入检测（InputModule）与输入响应模块（Selectable等组件）调用用来更换EventSystem当前对象时被触发的事件类型。

```C#
//EventSystem
//输入检测与输入响应的模块调用,变化当前选中的GameObject。
//每次只能有一个选中物体
public void SetSelectedGameObject(GameObject selected, BaseEventData pointer)
{
    if (m_SelectionGuard)
    {
        Debug.LogError("Attempting to select " + selected +  "while already selecting an object.");
        return;
    }

    m_SelectionGuard = true;
    if (selected == m_CurrentSelected)
    {
        m_SelectionGuard = false;
        return;
    }
	//对当前选中的物体执行 deselect事件
    ExecuteEvents.Execute(m_CurrentSelected, pointer, ExecuteEvents.deselectHandler);
    m_CurrentSelected = selected;
    //新选中的物体执行 select事件
    ExecuteEvents.Execute(m_CurrentSelected, pointer, ExecuteEvents.selectHandler);
    m_SelectionGuard = false;
}
```



### Selectable的生命周期

**Enable**:初始化过程会向一个静态 **List\<Selectable>**中添加自己，这是为**IMoveHandle**服务。当接收到方向键移动事件时，在导航(**Navigation下文会介绍到**)失败后根据输入方向和当前Selectable组件的坐标会遍历List寻找，最终找到满足条件的新目标Selectable组件。

```C#
protected override void OnEnable()
{
    base.OnEnable();
    s_List.Add(this); //在静态链表中添加自己
    var state = SelectionState.Normal;
    if (hasSelection)
        state = SelectionState.Highlighted;
    m_CurrentSelectionState = state;//设置选中状态
    //状态变化的参数变化以及过渡(例如Highlighted 颜色需要变为highlightedColor所设置的颜色)
    InternalEvaluateAndTransitionToSelectionState(true);
}
```

**Disable**:将自己从链表中移除，并清楚自身状态(存在过渡)。

从**Selectable**的生命周期可以看出，它在通用处理两件事情：**导航(Navigation)选择目标** 与 **状态变化的过渡(Transition)**。顺着这两件事情，我们接着看。

### Navigation

**导航有五种类型可供选择**

```C#
public enum Mode
{
    None        = 0, // 无导航
    Horizontal  = 1, // 自动地水平方向导航
    Vertical    = 2, // 自动地垂直方向导航
    Automatic   = 3, // 自动两个维度的导航
    Explicit    = 4, // 自定义各个方向的导航目标
}
```

**当无导航时 None:**不会成为别的导航目标，自身也不会导航其他物体。

**当处于水平、垂直、二维导航模式下：**目标会根据距离来确定导航上下左右的导航目标（水平只有左右，垂直只有上下）。

**当处于自定义模式下：**可自己选择上下左右所导航的目标。



**当响应OnMove事件时，会根据导航模式寻找对应方向的目标**

```C#
public virtual void OnMove(AxisEventData eventData)
{
    switch (eventData.moveDir)
    {
        case MoveDirection.Right:
            Navigate(eventData, FindSelectableOnRight());//寻找右侧目标
            break;
		....
    }
}
```

```C#
public virtual Selectable FindSelectableOnRight()
{
    if (m_Navigation.mode == Navigation.Mode.Explicit)
    {
        //若导航是自定义模式，则至今返回设置的右侧目标
        return m_Navigation.selectOnRight;
    }
    if ((m_Navigation.mode & Navigation.Mode.Horizontal) != 0)
    {
        //导航满足方向条件时，根据世界坐标遍历List<Selectable>寻找下一个可选择的对象
        return FindSelectable(transform.rotation * Vector3.right);
    }
    return null;
}
```

### Transition

**状态过渡有四种类型可供选择**

```c#
public enum Transition
{
    None,//无
    ColorTint,//颜色变化
    SpriteSwap,//Sprite图像变化
    Animation // 动画变化
}
```

**当Selectable状态改变时，会改变其状态相关属性(颜色、图像、动画Trigger)，并执行对应过渡。**

```C#
protected virtual void DoStateTransition(SelectionState state, bool instant)
{
    Color tintColor;
    Sprite transitionSprite;
    string triggerName;
	
    //.... 省略中间根据类型的属性赋值
        
    if (gameObject.activeInHierarchy)
    {
        switch (m_Transition)
        {
            case Transition.ColorTint:
                StartColorTween(tintColor * m_Colors.colorMultiplier, instant);
                break;
            case Transition.SpriteSwap:
                DoSpriteSwap(transitionSprite);
                break;
            case Transition.Animation:
                TriggerAnimation(triggerName);
                break;
        }
    }
}
```

**颜色变化：通过Graphic中的CrossFadeColor方法，这是通过TweenRunner来实现的携程动画。**

```C#
void StartColorTween(Color targetColor, bool instant)
{
    if (m_TargetGraphic == null)
        return;
    m_TargetGraphic.CrossFadeColor(targetColor, instant ? 0f : m_Colors.fadeDuration, true, true);
}
```

**图像变化：非常简单，将新图像设置给Image**

```C#
void DoSpriteSwap(Sprite newSprite)
{
    if (image == null)
        return;
    image.overrideSprite = newSprite;
}
```

**动画变化：执行Animator的Trigger操作**

```C#
void TriggerAnimation(string triggername)
{
    if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
        return;
    animator.ResetTrigger(m_AnimationTriggers.normalTrigger);
    animator.ResetTrigger(m_AnimationTriggers.pressedTrigger);
    animator.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
    animator.ResetTrigger(m_AnimationTriggers.disabledTrigger);

    animator.SetTrigger(triggername);
}
```



### 事件响应

Selectable虽然继承了许多的事件处理接口，但是对事件的响应并没有做太多的处理，仅仅只是为它的子类们做好了通用部分的处理(**改变状态并执行对应过渡**)，具体对事件的处理交给了它的子类们。







***



# Graphic Component

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

## Text

> **BaseClass: MaskableGraphic**
>
> **Interface: ,ILayoutElement**
>
> **Intro: UGUI中重要的组件之一， 根据字符串显示文本的组件**

- **ILayoutElement**：布局元素，可以被布局组件(ILayoutController)布局

### FontData

**Text**，相比大家也再熟悉不过了。我们经常使用它来显示文字信息，并且Text组件提供了许多参数丰富显示内容与状态。这些参数主要来自于Text中的**FontData**。

`[SerializeField] private FontData m_FontData = FontData.defaultFontData;`

```C#
public class FontData : ISerializationCallbackReceiver
{
    //当这些值发生改变时，会触发Text的重建
    private Font m_Font; //字体
    private int m_FontSize; //字号
    private FontStyle m_FontStyle; //字形(加粗、倾斜)
    private bool m_BestFit; //设置最佳尺寸(根据矩形框大小改变字号大小),和overflow互斥
    private int m_MinSize;  // bestFit 的最小尺寸
    private int m_MaxSize;	// bestFit 的最大尺寸
    private TextAnchor m_Alignment; //文本对齐(控制文本居中/居右/居左...)
    private bool m_AlignByGeometry; //更具字符产生的矩形范围来进行对齐(下文具体介绍)
    private bool m_RichText; //富文本
    private HorizontalWrapMode m_HorizontalOverflow; //水平溢出
    private VerticalWrapMode m_VerticalOverflow; // 垂直溢出
    private float m_LineSpacing; // 行间距
	
    //默认参数设置
    public static FontData defaultFontData
    {
        get
        {
            var fontData = new FontData
            {
                m_FontSize  = 14,
                m_LineSpacing = 1f,
                m_FontStyle = FontStyle.Normal,
                m_BestFit = false,
                m_MinSize = 10,
                m_MaxSize = 40,
                m_Alignment = TextAnchor.UpperLeft,
                m_HorizontalOverflow = HorizontalWrapMode.Wrap,
                m_VerticalOverflow = VerticalWrapMode.Truncate,
                m_RichText  = true,
                m_AlignByGeometry = false
            };
            return fontData;
        }
    }

    ......
}
```



**AlignByGeometry:**

一般的，因为在不同字体中，文字的长宽是不相等的(**如果相等的话那文字之间的间距就会不一样**)，所以文本对齐的实现是选择**字体中的长宽最大值**为参考来进行对齐的，如图所示，采取的是左对齐与上对齐，**我们可以看见字与矩形的边界还是有一定距离的。**

![](G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\t1.png)

除了以上所说的常规对齐模式，Text还提供了一个布尔值变量**AlignByGeometry**来改变对齐模式。

**在AlignByGeometry开启的情况下**，Text会以**当前所显示的文字中获取最大长宽**作为参考（而不是字体中的长宽），进行对齐。（**图中锁定的是“$”字符，使其贴边**）

![](G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\t2.png)

**当“$”字符被删除后，会变更长宽参考，重新对齐。**

![](G:\Vin129P\PMP\PMP\Assets\9.UGUI\Texture\t3.png)



### Text的生命周期

​	**Enable**时会将 **cachedTextGenerator**设置成无效(这会使**TextGenerator**在调用 **Populate** 时强制生成全文本)。并向**FontUpdateTracker**注册自己（当Unity需要刷新字体资源时，用来帮助刷新Text组件）。

​	**Disable**时将自身移除出**FontUpdateTracker**。

**TextGenerator：**Unity专门用来渲染文字的类，在UnityEngine.dll。可以说Text的代码量并不是很多的原因是由该类完成了文本显示所需的顶点、字符信息、行信息的计算。

### OnPopulateMesh

​	进入这里，便是**Text**最为核心的方法，网格处理。不同于**Image**不仅要处理网格还要处理材质和纹理，**Text**并不需要对材质纹理做更加具体的处理，因为Text的材质默认来自于它的字体本身或是它身上所被设置的材质。

```C#
public override Texture mainTexture
{
    get
    {
        if (font != null && font.material != null && font.material.mainTexture != null)
            return font.material.mainTexture; //字体的材质

        if (m_Material != null)
            return m_Material.mainTexture; // 继承自Graphic,可以手动添加的材质

        return base.mainTexture;
    }
}
```

​	**Text重载了Graphic的OnPopulateMesh方法，该方法是几何更新生成网格（Mesh）的重要方法。通过TextGenerator计算顶点数量与位置（每个字符由四个顶点构成两个三角面）。**

```C#
protected override void OnPopulateMesh(VertexHelper toFill)
{
    if (font == null)
        return;
    m_DisableFontTextureRebuiltCallback = true;

    Vector2 extents = rectTransform.rect.size;

    var settings = GetGenerationSettings(extents);
    //根据字符串和FontData中的设置计算顶点数据
    cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
    // Apply the offset to the vertices
    IList<UIVertex> verts = cachedTextGenerator.verts;
    //m_FontData.fontSize/font.fontSize 字号放大缩小字体的具体实现来自这里，改变顶点位置
    float unitsPerPixel = 1 / pixelsPerUnit; 
    //Last 4 verts are always a new line... (\n)
    //这里可以看出来TextGenerator会生成 (字符数+1)*4 的顶点数据
    int vertCount = verts.Count - 4;

     //计算距离矩形边框的偏移量
    Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
    roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
    toFill.Clear();
    //设置顶点数据
    if (roundingOffset != Vector2.zero)
    {
        for (int i = 0; i < vertCount; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = verts[i];
            m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
            m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
            m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
            if (tempVertsIndex == 3)
                toFill.AddUIVertexQuad(m_TempVerts);
        }
    }
    else
    {
        for (int i = 0; i < vertCount; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = verts[i];
            m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
            if (tempVertsIndex == 3)
                toFill.AddUIVertexQuad(m_TempVerts);
        }
    }
    m_DisableFontTextureRebuiltCallback = false;
}
```



***

## Shadow

> **BaseClass: BaseMeshEffect**
>
> **Interface: None**
>
> **Intro: UGUI实现阴影效果组件**

基于网格实现阴影效果，在Graphic中DoMeshGeneration方法中（UpdateGeometry流程）会遍历所有IMeshModifier组件来更新最终的网格数据（VertexHelper）。

具体实现为：**根据之前顶点信息复制并进行偏移与改变颜色生成新的顶点并添加进当前网格数据中。**

即：**使用Shadow组件后，顶点数三角面数增加了一倍**

```C#
public override void ModifyMesh(VertexHelper vh)
{
    if (!IsActive())
        return;
    var output = ListPool<UIVertex>.Get();
    vh.GetUIVertexStream(output);//获取当前的顶点数据

    //通过该方法计算出新的顶点数据：复制当前顶点并,根据effectDistance位置偏移，根据effectColor改变颜色。 
    ApplyShadow(output, effectColor, 0, output.Count, effectDistance.x, effectDistance.y);
    vh.Clear();
    //添加进当前网格数据中
    vh.AddUIVertexTriangleStream(output);
    ListPool<UIVertex>.Release(output);
}
```



## Outline

> **BaseClass: Shadow**
>
> **Interface: None**
>
> **Intro: UGUI实现描边效果组件**

基于网格实现描边效果，继承自Shadow，仅仅只是在顶点处理上增加了4倍的顶点量。

具体实现为：**根据之前顶点信息复制并进行偏移与改变颜色生成新的顶点并添加进当前网格数据中。**

即：**使用Outline组件后，顶点数三角面数增加了四倍**

```C#
public override void ModifyMesh(VertexHelper vh)
{
    if (!IsActive())
        return;
    var verts = ListPool<UIVertex>.Get();
    vh.GetUIVertexStream(verts);
    var neededCpacity = verts.Count * 5;
    if (verts.Capacity < neededCpacity)
        verts.Capacity = neededCpacity;
    
    //改变偏移数据生成4倍的顶点数据
    var start = 0;
    var end = verts.Count;
    ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, effectDistance.y);

    start = end;
    end = verts.Count;
    ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, effectDistance.x, -effectDistance.y);

    start = end;
    end = verts.Count;
    ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, effectDistance.y);

    start = end;
    end = verts.Count;
    ApplyShadowZeroAlloc(verts, effectColor, start, verts.Count, -effectDistance.x, -effectDistance.y);

    vh.Clear();
    //添加进原始数据中
    vh.AddUIVertexTriangleStream(verts);
    ListPool<UIVertex>.Release(verts);
}
```



***

# Selectable Component

## Button

> **BaseClass: Selectable**
>
> **Interface: IPointerClickHandler,ISubmitHandler**
>
> **Intro: UGUI按钮点击组件**

- **IPointerClickHandler**：点击事件的响应接口
- **ISubmitHandler**：**Submit**按键点击事件的响应接口，**Submit**是可以在**Project Settings**中的**Input**输入设置。当组件被选中时（“选中”的详细介绍请看Selectable）可响应Submit事件。

**Button**,我们再熟悉不过的组件了。它完成了最简单的交互操作：**点击**。其实**Button**组件的源码非常简单，仅仅是实现了**OnPointerClick**与**OnSubmit**两个事件的响应。这个估计使用过Button的人都已经非常清楚不过了。

```C#
public virtual void OnSubmit(BaseEventData eventData)
{
    Press();//执行注册方法的逻辑
    if (!IsActive() || !IsInteractable())
        return;
    DoStateTransition(SelectionState.Pressed, false);
    //因为Selectable中已经写了OnPointerDown、OnPointerUp时对应的状态变化了
    //但是对于Submit并没有结束的判断事件，所以依靠协程来执行状态变化
    StartCoroutine(OnFinishSubmit());
}
```

```C#
public virtual void OnPointerClick(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
    Press();//执行注册方法的逻辑
}
```

```C#
//OnPointerClick 与 OnSubmit 都会执行的响应操作
private void Press()
{
    if (!IsActive() || !IsInteractable())
        return;
    UISystemProfilerApi.AddMarker("Button.onClick", this);
    m_OnClick.Invoke(); // 执行注册的方法
}
```



***

## Toggle

> **BaseClass: Selectable**
>
> **Interface: IPointerClickHandler,ISubmitHandler，ICanvasElement**
>
> **Intro: UGUI中单选多选开关组件**

- **IPointerClickHandler**：点击事件的响应接口
- **ISubmitHandler**：**Submit**按键点击事件的响应接口。
- **ICanvasElement:**  Canvas元素(重建接口)，当Canvas发生更新时执行重建操作

**Toggle**组件，UGUI中常用于控制单选或者多选功能的组件，经常与**ToggleGroup**一起使用。

**主要是通过一个bool值m_IsOn进行两种状态的切换（True/False），并通过一个监听事件传递状态的变化。**

**初始化过程：**

**Enable**阶段主要时将自身注册进**ToggleGroup**中，并根据当前状态执行特效变化。

**Disable**阶段会将自身从当前**ToggleGroup**组件中移除。

```C#
protected override void OnEnable()
{
    base.OnEnable();
    SetToggleGroup(m_Group, false);//含有ToggleGroup组件时，将自身注册进ToggleGroup中
    PlayEffect(true);//执行变化特效，渐变graphic的透明度
}

protected override void OnDisable()
{
    SetToggleGroup(null, false);//从当前的ToggleGroup中移除该组件
    base.OnDisable();
}
```

**IsOn:**

toggle组件最核心的地方在于这个bool值，该值可以通过点击（OnPointerClick）、按键（OnSubmit）、以及ToggleGroup进行改变。当IsOn发生改变时:

```C#
//IsOn属性Set方法  value：变化值  sendCallback 是否执行监听事件：默认true
void Set(bool value,bool sendCallback)
{
    if (m_IsOn == value)
        return;
    m_IsOn = value;
    if (m_Group != null && IsActive())
    {
        //当存在Group时，自身的变化需要通知Group，使其控制其他Toggle的状态
        if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
        {
            m_IsOn = true;
            m_Group.NotifyToggleOn(this);
        }
    }
    //执行变化特效，渐变graphic的透明度0或1
    PlayEffect(toggleTransition == ToggleTransition.None);
    if (sendCallback)
    {
        UISystemProfilerApi.AddMarker("Toggle.value", this);
        onValueChanged.Invoke(m_IsOn);//执行监听事件
    }
}
```

**ToggleGroup:**

**ToggleGroup**组件用来帮助**Toggle捆绑成组**，使其完成**X选1**或者**多选**的功能。它管理了一个**List\<Toggle>**,当**Toggle**初始化时，会将自身注册进**List**中，到被销毁时会将自身移除。

**ToggleGroup**向外提供了一个**bool**值属性 **allowSwitchOff** 来控制**一组Toggle**中是否运行出现全是**OFF**的状态。

```C#
//Toggle组件IsOn属性Set方法中一段
if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
{
    //当Group.allowSwitchOff为false时将默认被操作的Toggle的IsOn设置为True
    m_IsOn = true;
    m_Group.NotifyToggleOn(this);
}
```

**在单选模式中（allowSwitchOff = false）**: 当一组Toggle中出现**点击或是Submit操作**时，会将其**IsOn**变化为**True**状态并执行Group的**NotifyToggleOn**方法，**该方法会遍历其List中除此之外的所有Toggle，使它们变为False状态**。

```C#
public void NotifyToggleOn(Toggle toggle)
{
    ValidateToggleIsInGroup(toggle);//判断当前Toggle是否存在List中
    //遍历出当前Toggle外的所有Tgoole，改变它们的状态为False
    for (var i = 0; i < m_Toggles.Count; i++)
    {
        if (m_Toggles[i] == toggle)
            continue;
        m_Toggles[i].isOn = false;
    }
}
```



***

## Scrollbar

> **BaseClass: Selectable**
>
> **Interface: IInitializePotentialDragHandler,IBeginDragHandler，IDragHandler,ICanvasElement**
>
> **Intro: UGUI中滚动条组件**

- **initializePotentialDrag**：提前告知可能触发拖拽的接口，这个接口只有在存在**IDragHandler**接口时才会触发，当点击或触碰时便触发了（**会发生在BeginDrag之前**）
- **IXXXDragHandler**：三个拖拽接口，这里就简写成这样，监听整个拖拽过程（开始，拖拽，结束）。
- **ICanvasElement**： Canvas重建接口，当Canvas发生更新时执行重建操作

**Scrollbar**组件的实现原理：通过拖拽事件、点击事件、移动按键来控制滚动条在一定区域中移动，并计算位于该区域中（0~1）之间的值value。

**拖拽原理类似于ScrollRect，这个我们具体在ScrollRect中进行分析。**

**点击交互：**发生点击交互时，会根据点击位置启动一个携程来进行位置更新。

```C#
public override void OnPointerDown(PointerEventData eventData)
{
    //如果是要进行拖拽了那就没事了:)
    if (!MayDrag(eventData))
        return;
    base.OnPointerDown(eventData);//执行Selectable基类方法
    isPointerDownAndNotDragging = true; //标记状态
    m_PointerDownRepeat = StartCoroutine(ClickRepeat(eventData)); //启动变化携程
}
```

```C#
protected IEnumerator ClickRepeat(PointerEventData eventData)
{
    //当抬起操作时则此循环结束
    while (isPointerDownAndNotDragging)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
        {
            Vector2 localMousePos;
            //获取鼠标点击位于HandleRect的本地坐标
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
            {
                //根据轴变化value值,频率取决于size大小
                var axisCoordinate = axis == 0 ? localMousePos.x : localMousePos.y;
                if (axisCoordinate < 0)
                    value -= size;
                else
                    value += size;
            }
        }
        yield return new WaitForEndOfFrame();
    }
    StopCoroutine(m_PointerDownRepeat);
}
```



***



## ScrollRect

> **BaseClass: UIBehaviour**
>
> **Interface: IInitializePotentialDragHandler,IXXXDragHandler，IScrollHandler,ICanvasElement,ILayoutElement,ILayoutGroup**
>
> **Intro: UGUI中滑动列表组件**

- **initializePotentialDrag**：提前告知可能触发拖拽的接口，这个接口只有在存在**IDragHandler**接口时才会触发，当点击或触碰时便触发了（**会发生在BeginDrag之前**）
- **IXXXDragHandler**：三个拖拽接口，这里就简写成这样，监听整个拖拽过程（开始，拖拽，结束）。
- **IScrollHandler:**  鼠标中间滚动事件接口
- **ICanvasElement**： Canvas重建接口，当Canvas发生更新时执行重建操作
- **ILayoutElement&ILayoutGroup**：布局相关接口

**ScrollRect**，是UGUI中滑动列表功能时经常被使用的组件。它提供了水平和垂直两种滑动模式，配合**Scrollbar**组件经常被用于UI制作。

**ScrollRect组件的主要原理：通过拖拽事件、滚轮事件、ScrollBar来控制一块区域的移动，达到滚动列表的效果。**

**初始化**

**Enable阶段**：向两个**Scrollbar**组件注册事件监听来激活滚动条的响应交互，并向重建系统（**CanvasUpdateSystem**）中的布局重建队（**m_LayoutRebuildQueue**）列添加自己。

```C#
protected override void OnEnable()
{
    base.OnEnable();
    //注册scrollbar的事件监听
    if (m_HorizontalScrollbar)
       m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
    if (m_VerticalScrollbar)
        m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
    //将自身添加进布局重建队列中
    CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
}
```

**Disable阶段**：则对Enable阶段的注册进行相对应的移除工作

**LateUpdate阶段**：这个阶段最为重要，在每帧的末尾根据组件所处在的状态（拖拽刚结束/正在拖拽）进行**数据更新**或着**模式补偿**处理。

```C#
protected virtual void LateUpdate()
{
    if (!m_Content)
        return;
    EnsureLayoutHasRebuilt();//确保布局刷新
    UpdateBounds();//更新两个包围盒的数据
    float deltaTime = Time.unscaledDeltaTime;
    //计算内容盒对于视图盒的偏移值，是否存在超出边界的部分
    Vector2 offset = CalculateOffset(Vector2.zero);
    //在拖拽结束之后判断移动模式，进行位置补差操作（弹性模式/惯性作用）
    if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
    {
        Vector2 position = m_Content.anchoredPosition;
        for (int axis = 0; axis < 2; axis++)
        {
            //在弹性模式下且存在偏移时，进行回弹操作
            if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
            {
                float speed = m_Velocity[axis];
                position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                if (Mathf.Abs(speed) < 1)
                    speed = 0;
                m_Velocity[axis] = speed;
            }
            //在惯性下，对内容区域进行减速运动
            else if (m_Inertia)
            {
                m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                if (Mathf.Abs(m_Velocity[axis]) < 1)
                    m_Velocity[axis] = 0;
                position[axis] += m_Velocity[axis] * deltaTime;
            }
            else
            {
                m_Velocity[axis] = 0;
            }
        }
        //强制模式下则不需过渡直接回正偏移
        if (m_MovementType == MovementType.Clamped)
        {
            offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
        }
        SetContentAnchoredPosition(position);
    }
    //当正在发生拖拽时且是惯性状态下，需要计算拖拽速度
    if (m_Dragging && m_Inertia)
    {
        Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
        m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
    }
    if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
    {
        //更新Scrollbar的value值
        UpdateScrollbars(offset);
        UISystemProfilerApi.AddMarker("ScrollRect.value", this);
        //执行位置变化的监听事件
        m_OnValueChanged.Invoke(normalizedPosition);
        UpdatePrevData();
    }
    UpdateScrollbarVisibility();
}
```





**注**：**ScrollRect**中，使用了**Bounds**结构来进行位置变化的判断与计算（**Bounds** 是Unity封装的用于区域计算的结构体，它是轴对称性质的空间，所以只需要中心与尺寸即可）。

**viewBounds 对应视图区域的包围空间（以下会简称视图盒）**

**contentBounds 对应内容区域的包围空间（简称内容盒）**

***

**滑动条交互**

这里我们选择**HorizontalScrollbar**作为示例进行分析

- 在初始化时会注册事件监听，来响应Scrollbar的value值变化

  `m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);`

- 当value发生变化时，通过该value值对应的内容盒的偏移值可以计算出内容区域的新坐标（基于ViewRect的本地坐标）。

  ```C#
  // axis 代表坐标参数 0为x轴，1为y轴
  protected virtual void SetNormalizedPosition(float value, int axis)
  {
      EnsureLayoutHasRebuilt();
      UpdateBounds();
      // 计算scrollbar value值控制的长度大小，即 内容区域大于试图区域的大小
      float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
      // 计算内容盒最小坐标改变多少
      float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
      // 计算出内容区域的最新本地坐标
      float newLocalPosition = m_Content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];
  
      Vector3 localPosition = m_Content.localPosition;
      //判断变化大小是否合理，太小的情况将被忽略
      if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
      {
          //更新坐标位置
          localPosition[axis] = newLocalPosition;
          m_Content.localPosition = localPosition;
          m_Velocity[axis] = 0;
          UpdateBounds();
      }
  }
  ```

  

***

**拖拽交互**

拖拽交互分为四个部分分布由四个接口事件依次响应。

- **即将发生拖拽**：当鼠标左键或者触点接触**ScrollRect**时会触发**InitializePotentialDrag**，将速度值归零，准备进行拖拽事件。

```C#
public virtual void OnInitializePotentialDrag(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
	//速度值归零
    m_Velocity = Vector2.zero;
}
```

- **开始拖拽**：初始化两个参数 m_PointerStartLocalCursor 起始拖拽点坐标  m_ContentStartPosition 内容区域起始位置

```C#
public virtual void OnBeginDrag(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
    if (!IsActive())
        return;
    //更新两个包围盒的数据
    UpdateBounds();
    m_PointerStartLocalCursor = Vector2.zero;
    //记录由屏幕坐标转换为视图区域下的起始位置坐标
    RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
    //记录内容区域当前坐标
    m_ContentStartPosition = m_Content.anchoredPosition;
    //标记正在进行拖拽
    m_Dragging = true;
}
```

- **拖拽过程**：根据**PointerEventData**中的数据，计算拖拽导致内容区域的坐标变化

```C#
public virtual void OnDrag(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
    if (!IsActive())
        return;
    Vector2 localCursor;
    //获取当前触点位于视图区域中的坐标
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
        return;
    UpdateBounds();
    //与起始坐标求插值
    var pointerDelta = localCursor - m_PointerStartLocalCursor;
    //计算拖拽变化后的区域位置
    Vector2 position = m_ContentStartPosition + pointerDelta;

    // 计算内容区域在视图区域下是否需要偏移量,即是否有超出边界的情况
    Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
    position += offset;
    if (m_MovementType == MovementType.Elastic)
    {
        //当处于弹性模式下时,会根据偏移量增加一个弹性势能让内容区域不会全部处于视图区域的外部
        if (offset.x != 0)
            position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);//这里随着offset值越大势能越强
        if (offset.y != 0)
            position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
    }
    //更新内容区域坐标
    SetContentAnchoredPosition(position);
}
```

- **拖拽结束**：改变拖拽状态，并在**LateUpdate**中进行拖拽结束后的补偿操作（例如：**弹性模式下的回弹**，**惯性下的减速运动**）

```C#
public virtual void OnEndDrag(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
    //改变拖拽状态
    m_Dragging = false;
}
```



***

**滚轮交互**

滚轮交互特点在于无法控制垂直开关与水平开关同时存在的ScrollRect。

```C#
public virtual void OnScroll(PointerEventData data)
{
    if (!IsActive())
        return;
    EnsureLayoutHasRebuilt();
    UpdateBounds();
    //获取滚轮变化值
    Vector2 delta = data.scrollDelta;
    delta.y *= -1;
    //滚轮交互支持水平与垂直同时存在的情况
    if (vertical && !horizontal)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            delta.y = delta.x;
        delta.x = 0;
    }
    if (horizontal && !vertical)
    {
        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            delta.x = delta.y;
        delta.y = 0;
    }
    //根据滚轮值变化计算内容区域位置变化
    Vector2 position = m_Content.anchoredPosition;
    position += delta * m_ScrollSensitivity;
    //如果是强制模式下,确保不会使内容区域超出边界
    if (m_MovementType == MovementType.Clamped)
        position += CalculateOffset(position - m_Content.anchoredPosition);
    SetContentAnchoredPosition(position);
    UpdateBounds();
}
```

***

## Dropdown

> **BaseClass: Selectable**
>
> **Interface:IPointerClickHandler,ISubmitHandler,ICancelHandler**
>
> **Intro: UGUI中下拉列表组件**

- **IPointerClickHandler**：点击事件的响应接口
- **ISubmitHandler**：**Submit**按键点击事件的响应接口，**Submit**是可以在**Project Settings**中的**Input**输入设置。当组件被选中时（“选中”的详细介绍请看Selectable）可响应Submit事件。
- **ICancelHandler** ：**Cancel**按键点击事件的响应接口，原理同**Submit**接口，此按键代表取消操作

**Dropdown**，是UGUI中下拉列表功能组件。它属于直接介绍的组件的混合体，**Dropdown**组件中运用到了**Text、Toggle、Scrollbar、ScrollRect**组件，更具具体需求可以舍弃**除Toggle**之外的组件进行自己的改造。

**初始化**

​	**Dropdown**的初始化过程仅有一个**Awake**，做了帮助实现动画过渡模块初始化。

```C#
protected override void Awake()
{
    #if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
    #endif
        m_AlphaTweenRunner = new TweenRunner<FloatTween>();
    m_AlphaTweenRunner.Init(this); // 初始化渐变模块
    if (m_CaptionImage)
        m_CaptionImage.enabled = (m_CaptionImage.sprite != null);
    if (m_Template)
        m_Template.gameObject.SetActive(false);
}
```



**工作原理**

​	本篇文章着重分析**Dropdown**组件下拉列表功能的实现。

**由事件为导向的触发**：在**点击事件**与**确定事件**中都是相同的处理，执行了**Show()**方法显示下拉列表

```C#
public virtual void OnPointerClick(PointerEventData eventData)
{
    Show();
}
public virtual void OnSubmit(BaseEventData eventData)
{
    Show();
}
```

**Show()方法**：显示下拉列表的方法，也是整个Dropdown组件最关键的方法，它做到了以下几点内容

1. 若没有对列表模板（**Template**）进行过**初始化设置**便进行初始化：检测模板是否符合要求（**Item含有Toggle组件、父级不是RectTransform、ItemText与ItemImage如果存在必须在Item内部**），为**ToggleItem**添加**DropdownItem**组件并做相关绑定，将列表模板处于UI的最高层（`popupCanvas.sortingOrder = 30000;`）

2. **复制模板与创建Item**：此过程会根据选项内容数据调整区域大小。

3. **创建阻拦层**：阻拦层是用于监听用户点击事件并执行下拉列表的隐藏，它的层级仅次于列表层（29999）

```c#
public void Show()
{
    if (!IsActive() || !IsInteractable() || m_Dropdown != null)
        return;
    //初始状态时validTemplate为false来触发对于列表模板的初始化设置
    if (!validTemplate)
    {
        //模板初始化方法：检测并设置模板，初始化模板绑定相关组件并调整模板UI层级，若没有通过检查则模板标记为不可用状态。
        SetupTemplate();
        //若检测不通过则无法正常显示下拉列表
        if (!validTemplate)
            return;
    }

    var list = ListPool<Canvas>.Get();
    gameObject.GetComponentsInParent(false, list);
    if (list.Count == 0)
        return;
    //获取父级路径下最近的canvas
    Canvas rootCanvas = list[0];
    ListPool<Canvas>.Release(list);
    //显示模板准备复制列表
    m_Template.gameObject.SetActive(true);
    //复制列表模板
    m_Dropdown = CreateDropdownList(m_Template.gameObject);
    //进行改名
    m_Dropdown.name = "Dropdown List";
    m_Dropdown.SetActive(true);

    // 设置新的列表模板的父级
    RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
    dropdownRectTransform.SetParent(m_Template.transform.parent, false);
    // 创建列表Item
    DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();

    GameObject content = itemTemplate.rectTransform.parent.gameObject;
    RectTransform contentRectTransform = content.transform as RectTransform;
    itemTemplate.rectTransform.gameObject.SetActive(true);

    Rect dropdownContentRect = contentRectTransform.rect;
    Rect itemTemplateRect = itemTemplate.rectTransform.rect;

    //计算Item与背景边界的偏移量
    Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
    Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
    Vector2 itemSize = itemTemplateRect.size;

    //清空DropdownItem List 准备开始选项Itme的创建
    m_Items.Clear();

    Toggle prev = null;
    for (int i = 0; i < options.Count; ++i)
    {
        OptionData data = options[i];
        //创建Item
        DropdownItem item = AddItem(data, value == i, itemTemplate, m_Items);
        if (item == null)
            continue;
        // 设置toggle初始状态以及注册事件监听
        item.toggle.isOn = value == i;
        item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));
        //标记当前选项
        if (item.toggle.isOn)
            item.toggle.Select();
        // 设置Item的导航
        if (prev != null)
        {
            Navigation prevNav = prev.navigation;
            Navigation toggleNav = item.toggle.navigation;
            prevNav.mode = Navigation.Mode.Explicit;
            toggleNav.mode = Navigation.Mode.Explicit;

            prevNav.selectOnDown = item.toggle;
            prevNav.selectOnRight = item.toggle;
            toggleNav.selectOnLeft = prev;
            toggleNav.selectOnUp = prev;

            prev.navigation = prevNav;
            item.toggle.navigation = toggleNav;
        }
        prev = item.toggle;
    }
    // 计算内容区域的高度
    Vector2 sizeDelta = contentRectTransform.sizeDelta;
    sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
    contentRectTransform.sizeDelta = sizeDelta;

    //计算是否有额外空区域（当内容区域小于列表本身的区域时调整列表大小）
    float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
    if (extraSpace > 0)
        dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

    // 当列表处于canvas外部时，将其按坐标轴进行翻转
    Vector3[] corners = new Vector3[4];
    dropdownRectTransform.GetWorldCorners(corners);

    RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
    Rect rootCanvasRect = rootCanvasRectTransform.rect;
    for (int axis = 0; axis < 2; axis++)
    {
        bool outside = false;
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
            if (corner[axis] < rootCanvasRect.min[axis] || corner[axis] > rootCanvasRect.max[axis])
            {
                outside = true;
                break;
            }
        }
        if (outside)
            RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
    }

    for (int i = 0; i < m_Items.Count; i++)
    {
        RectTransform itemRect = m_Items[i].rectTransform;
        itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
        itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
        itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
        itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
    }

    // 下拉列表渐出效果
    AlphaFadeList(0.15f, 0f, 1f);
    // 隐藏模板
    m_Template.gameObject.SetActive(false);
    itemTemplate.gameObject.SetActive(false);
    // 创建拦截模板,用于监听点击事件来隐藏下拉列表,层级会低于下拉列表(2999)
    m_Blocker = CreateBlocker(rootCanvas);
}
```

当下拉列表中的Item被选择时（即Toggle监听事件触发时）会执行选择操作，**更新选项值并隐藏列表**

```C#
private void OnSelectItem(Toggle toggle)
{
    if (!toggle.isOn)
        toggle.isOn = true;
    int selectedIndex = -1;
    Transform tr = toggle.transform;
    Transform parent = tr.parent;
    for (int i = 0; i < parent.childCount; i++)
    {
        if (parent.GetChild(i) == tr)
        {
            selectedIndex = i - 1;
            break;
        }
    }

    if (selectedIndex < 0)
        return;
	//更新 当前选项值
    value = selectedIndex;
    // 隐藏下拉列表
    Hide();
}
```

**value**值发生变化时，会执行**RefreshShownValue()**刷新显示。

```C#
//更新当前选项的信息至captionText、captionImage
public void RefreshShownValue()
{
    OptionData data = s_NoOptionData;
    if (options.Count > 0)
        data = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
    if (m_CaptionText)
    {
        if (data != null && data.text != null)
            m_CaptionText.text = data.text;
        else
            m_CaptionText.text = "";
    }
    if (m_CaptionImage)
    {
        if (data != null)
            m_CaptionImage.sprite = data.image;
        else
            m_CaptionImage.sprite = null;
        m_CaptionImage.enabled = (m_CaptionImage.sprite != null);
    }
}
```



***

## InputField

> **BaseClass: Selectable**
>
> **Interface: IUpdateSelectedHandler,IXXXDragHandler，IPointerClickHandler,ISubmitHandler,ICanvasElement,ILayoutElement**
>
> **Intro: UGUI中输入框组件组件**

- **IUpdateSelectedHandler**：刷新当前被选中物体事件监听，帧刷新
- **IXXXDragHandler**：三个拖拽接口，这里就简写成这样，监听整个拖拽过程（开始，拖拽，结束）。
- **IPointerClickHandler:**  点击事件接口
- **ISubmitHandler**： **Submit**按键点击事件的响应接口，**Submit**是可以在**Project Settings**中的**Input**输入设置。当组件被选中时（“选中”的详细介绍请看Selectable）可响应Submit事件。
- **ICanvasElement** ：Canvas元素(重建接口)，当Canvas发生更新时重建（void Rebuild）
- **ILayoutElement**：布局相关接口

**InputField**，是UGUI中输入文本框组件。它提供了**丰富的输入文本属性**来实现输入功能。

为了满足各种要求的输入，**InputField**的代码量非常的大（**2400+行**）是目前UGUI组件中**内容量最大**的组件，逻辑并不复杂，只在于它实现的输入类型种类繁多所以初看源码会有一种**懵**的感觉。所以本章的分析思路主要围绕其主要的输入功能的实现过程，并针对一种输入类型进行分析，理清思路。

**初始化**

一如既往，阅读源码的最好方式就是先从生命周期开始。

**Enable阶段**：主要是向Text组件注册了重建前（脏标记）时的事件监听。并执行了**UpdateLabel**方法，相应的事件中也包含了**UpdateLabel**方法，该方法便是我们重点关注的地方。

```c#
protected override void OnEnable()
{
    base.OnEnable();
    if (m_Text == null)
        m_Text = string.Empty;
    m_DrawStart = 0;
    m_DrawEnd = m_Text.Length;

    if (m_CachedInputRenderer != null)     m_CachedInputRenderer.SetMaterial(m_TextComponent.GetModifiedMaterial(Graphic.defaultGraphicMaterial), Texture2D.whiteTexture);

    //向Text组件注册事件监听，主要当Graphic进行顶点与材质的重建标记时会执行相应的监听事件
    if (m_TextComponent != null)
    {
        m_TextComponent.RegisterDirtyVerticesCallback(MarkGeometryAsDirty);
        m_TextComponent.RegisterDirtyVerticesCallback(UpdateLabel);
        m_TextComponent.RegisterDirtyMaterialCallback(UpdateCaretMaterial);
        UpdateLabel();//更新文本
    }
}
```

**Disable阶段**：非常普通地做了一些清理工作。

```c#
protected override void OnDisable()
{
    // 关闭携程
    m_BlinkCoroutine = null;
    //将各种属性做无效处理
    DeactivateInputField();
    //注销Text中的事件监听
    if (m_TextComponent != null)
    {
        m_TextComponent.UnregisterDirtyVerticesCallback(MarkGeometryAsDirty);
        m_TextComponent.UnregisterDirtyVerticesCallback(UpdateLabel);
        m_TextComponent.UnregisterDirtyMaterialCallback(UpdateCaretMaterial);
    }
    CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
    if (m_CachedInputRenderer != null)
        m_CachedInputRenderer.Clear();
    //清除网格
    if (m_Mesh != null)
        DestroyImmediate(m_Mesh);
    m_Mesh = null;
    base.OnDisable();
}
```



**事件接口**

UI组件的交互都是基于输入事件的，我们按照使用**InputField**的操作流程来一一分析各个事件。

**第一步：点击激活InputField**

```c#
public virtual void OnPointerClick(PointerEventData eventData)
{
    if (eventData.button != PointerEventData.InputButton.Left)
        return;
	//激活输入框组件
    ActivateInputField();
}
```

**InputField**中使用了Unity提供的键盘接口类型（**TouchScreenKeyboard**），该接口记录了我们的输入内容。

```c#
public void ActivateInputField()
{
    if (m_TextComponent == null || m_TextComponent.font == null || !IsActive() || !IsInteractable())
        return;

    if (isFocused)
    {
        if (m_Keyboard != null && !m_Keyboard.active)
        {
            //激活键盘接口,并将当前输入框设置进键盘接口中
            m_Keyboard.active = true;
            m_Keyboard.text = m_Text;
        }
    }

    m_ShouldActivateNextUpdate = true;
}
```

到此，激活操作就结束了。主要内容是激活了**TouchScreenKeyboard**，来保存我们的输入。

**第二步：输入文字**

**InputField**采用了每帧更新的方式来更新我们的输入（**LateUpdate**）。

**获取键盘输入信息，并验证信息的有效字符，并更新。**

```c#
protected virtual void LateUpdate()
{
    if (m_ShouldActivateNextUpdate)
    {
        if (!isFocused)
        {
            ActivateInputFieldInternal();
            m_ShouldActivateNextUpdate = false;
            return;
        }
        m_ShouldActivateNextUpdate = false;
    }
    if (InPlaceEditing() || !isFocused)
        return;
    AssignPositioningIfNeeded();
    if (m_Keyboard == null || m_Keyboard.done)
    {
        if (m_Keyboard != null)
        {
            if (!m_ReadOnly)
                text = m_Keyboard.text;
            if (m_Keyboard.wasCanceled)
                m_WasCanceled = true;
        }
        OnDeselect(null);
        return;
    }
    //获取输入信息
    string val = m_Keyboard.text;
    if (m_Text != val)
    {
        //只读情况则无法改变输入内容
        if (m_ReadOnly)
        {
            m_Keyboard.text = m_Text;
        }
        else
        {
            m_Text = "";
            for (int i = 0; i < val.Length; ++i)
            {
                char c = val[i];
                if (c == '\r' || (int)c == 3)
                    c = '\n';
                //验证输入内容
                if (onValidateInput != null)
                    c = onValidateInput(m_Text, m_Text.Length, c);
                else if (characterValidation != CharacterValidation.None)
                    c = Validate(m_Text, m_Text.Length, c);//默认的验证方法
                //限制行时当存在换行情况则停止更新
                if (lineType == LineType.MultiLineSubmit && c == '\n')
                {
                    m_Keyboard.text = m_Text;

                    OnDeselect(null);
                    return;
                }
                if (c != 0)
                    m_Text += c;
            }
            //字数限制的情况进行切割
            if (characterLimit > 0 && m_Text.Length > characterLimit)
                m_Text = m_Text.Substring(0, characterLimit);

            if (m_Keyboard.canGetSelection)
            {
                UpdateCaretFromKeyboard();
            }
            else
            {
                caretPositionInternal = caretSelectPositionInternal = m_Text.Length;
            }
            if (m_Text != val)
                m_Keyboard.text = m_Text;
            //执行事件并更新文本显示
            SendOnValueChangedAndUpdateLabel();
        }
    }
    else if (m_Keyboard.canGetSelection)
    {
        UpdateCaretFromKeyboard();
    }

    //当键盘输入完毕时，执行Deselect,会关闭键盘并执行编辑完成的事件
    if (m_Keyboard.done)
    {
        if (m_Keyboard.wasCanceled)
            m_WasCanceled = true;

        OnDeselect(null);
    }
}
```

**更新显示内容**


```C#
protected void UpdateLabel()
{
    if (m_TextComponent != null && m_TextComponent.font != null && !m_PreventFontCallback)
    {
        m_PreventFontCallback = true;

        string fullText;
        if (compositionString.Length > 0)
            fullText = text.Substring(0, m_CaretPosition) + compositionString + text.Substring(m_CaretPosition);
        else
            fullText = text;

        string processed;
        //输入类型为密码类型时，用*替换
        if (inputType == InputType.Password)
            processed = new string(asteriskChar, fullText.Length);
        else
            processed = fullText;

        bool isEmpty = string.IsNullOrEmpty(fullText);

        if (m_Placeholder != null)
            m_Placeholder.enabled = isEmpty;

        if (!m_AllowInput)
        {
            m_DrawStart = 0;
            m_DrawEnd = m_Text.Length;
        }

        if (!isEmpty)
        {
            Vector2 extents = m_TextComponent.rectTransform.rect.size;

            var settings = m_TextComponent.GetGenerationSettings(extents);
            settings.generateOutOfBounds = true;
            //生成mesh数据
            cachedInputTextGenerator.PopulateWithErrors(processed, settings, gameObject);
            //计算光标位置
            SetDrawRangeToContainCaretPosition(caretSelectPositionInternal);

            processed = processed.Substring(m_DrawStart, Mathf.Min(m_DrawEnd, processed.Length) - m_DrawStart);
            //通过携程显示光标
            SetCaretVisible();
        }
        m_TextComponent.text = processed;
        MarkGeometryAsDirty();
        m_PreventFontCallback = false;
    }
}
```

**第三步：结束编辑**

由**LateUpdate**检测**m_Keyboard.done**，或者执行**Deselect**事件时（变更目标时执行）

```c#
//取消inputfield的激活
public void DeactivateInputField()
{
    if (!m_AllowInput)
        return;
    //处理相关输入事件的属性,将其重置
    m_HasDoneFocusTransition = false;
    m_AllowInput = false;
    if (m_Placeholder != null)
        m_Placeholder.enabled = string.IsNullOrEmpty(m_Text);
    if (m_TextComponent != null && IsInteractable())
    {
        if (m_WasCanceled)
            text = m_OriginalText;
        if (m_Keyboard != null)
        {
            m_Keyboard.active = false;
            m_Keyboard = null;
        }
        m_CaretPosition = m_CaretSelectPosition = 0;
        //执行编辑完毕的事件监听
        SendOnSubmit();
        input.imeCompositionMode = IMECompositionMode.Auto;
    }

    MarkGeometryAsDirty();
}
```



***















***







***




# 资料链接

[processon](https://www.processon.com/diagraming/5e8953e5e4b0bf3ebcf8be7d)

[源码地址](https://bitbucket.org/Unity-Technologies/ui/src/2017.4/)

[UGUI使用教程](https://gameinstitute.qq.com/community/search?keyword=UGUI使用教程)

# 用时

**27h**