# 接入XLua
XLua的快速使用正如它的文档中所描述的那样，仅仅三行就完事了~
```csharp
XLua.LuaEnv luaenv = new XLua.LuaEnv();
luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaenv.Dispose();
```
接下来就是如何将这三行所包含的内容接入到我们的工程框架中了。

## 配置
在使用Unity做开发的过程中，多多少少在用到Lua时会需要和C#进行一些数据交互。在XLua中，无论是[LuaCallCSharp]还是[CSharpCallLua]