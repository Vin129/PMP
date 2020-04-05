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

> **BaseClass: UIBehaviour**
>
> **Interface: 无**
>
> **Intro: UGUI组件们的事件系统，是UGUI的感应神经。我们的操作会通过这个系统传递并响应在正确的组件上，从而让组件真正的被使用起来。**












## 资料链接

[源码地址](https://bitbucket.org/Unity-Technologies/ui/src/2017.4/)