# 接入XLua
XLua的快速使用正如它的文档中所描述的那样，仅仅三行就完事了~
```csharp
XLua.LuaEnv luaenv = new XLua.LuaEnv();
luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaenv.Dispose();
```
接下来就是如何将这三行所包含的内容接入到我们的工程框架中了。

## 配置
用过Lua来写逻辑代码的时候都会需要和C#进行一些数据交互，早期的Lua与C#间的交互通过反射来完成，但随着逻辑复杂度与代码量的上升你会发现这并不是一种良性的交互方式。随后变出现了另一种交互方式：