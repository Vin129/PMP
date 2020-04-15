# UGUI

如果你在正在使用Unity，那你很难不去接触它的UI部分。

本专栏将针对Unity的UGUI源码对整个UGUI进行系统的讲解。

通过现象看本质，理解掌握，使你更加得心应手。

## Base

### UIBehaviour

> **BaseClass: MonoBehaviour**
>
> **Interface: 无**
>
> **Intro: UGUI组件的基础类，为UGUI组件提供了三个模块通用接口。**



1.MonoBehaviour 生命周期

2.UnityEditor 辅助方法

3.UGUI 通用方法



***

### EventSystem

> **Related Class: BaseInputModule、BaseEventData、 RaycasterManager+**
>
> **Related  Interface: 无**
>
> **Related Other: 无**
>
> **Intro: UGUI事件管理系统，通过此系统完成我们所知的UI交互。**

**EventSystem:  管理 所有的InputModule并推动Module的工作流（Process）**

#### **BaseInputModule **

**主要分为单点触控（StandaloneInputModule）与多点触控（~~TouchInputModule~~ Touch模块已经整合进Standalone中）模块。**

- **ProcessMouseEvent：处理所有鼠标事件**
- **ProcessTouchEvents：处理所有触点事件**

##### **ProcessMouseEvent**

- MouseState ：获取当前鼠标状态



**检查Input中各项数值 => 判断当前操作状态**







***

### Raycasters

> **Related Class:  BaseRaycaster+**
>
> **Related  Interface: 无**
>
> **Related Other: 无**
>
> **Intro: 事件系统的一部分，管理射线**

**RaycasterManager管理了一个 射线List**

- 射线启用时（Enable）加入List
- 射线弃用时（Disable）移除List



***

### CanvasUpdate System

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

CanvasUpdateRegistry 被初始化时向Canvas中注册了更新函数（PerformUpdate），触发重建。

```C#
Canvas.willRenderCanvases += PerformUpdate;
```

**PerformUpdate**

- 首先更新布局，根据父节点多少排序，由内向外更新。更新类型依次为 Prelayout 、Layout 、PostLayout（enum CanvasUpdate）
- 通知布局完成
- ClipperRegistry 进行剪裁（待之后补充）
- 更新图像，依次 PreRender、LatePreRender、MaxUpdateValue
- 通知图像更新完成





***





## Graphic

> **BaseClass: UIBehaviour**
>
> **Interface: ICanvasElement**
>
> **Intro: 图形组件的基类**

- **ICanvasElement**: Canvas元素，当Canvas发生更新时重建（void Rebuild）

### 脏标

Graphic 中存在三种脏标分别代表三种等待重建

1. 尺寸改变时（RectTransformDimensions）：LayoutRebuild 布局重建
2. 尺寸、颜色改变时：Vertices to GraphicRebuild  图像重建
3. 材质改变时：Material to GraphicRebuild  图像重建

层级改变、应用动画属性（DidApplyAnimationProperties） ：All to Rebuild 重建所有








## 资料链接

[processon](https://www.processon.com/diagraming/5e8953e5e4b0bf3ebcf8be7d)

[源码地址](https://bitbucket.org/Unity-Technologies/ui/src/2017.4/)



## 用时

**4h**