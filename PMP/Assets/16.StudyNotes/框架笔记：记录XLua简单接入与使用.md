# XLua
XLua的快速使用正如它的文档中所描述的那样，仅仅三行就完事了~
```csharp
XLua.LuaEnv luaenv = new XLua.LuaEnv();
luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");
luaenv.Dispose();
```
接下来就是如何将这三行所包含的内容接入到我们的工程框架中了。

# 配置
在使用Unity做开发的过程中，多多少少在用到Lua时会需要和C#进行一些数据交互。在XLua中，无论是[LuaCallCSharp]还是[CSharpCallLua]用到的一个“特性”叫**Wrap**。

**Wrap**,可以理解成**“包装”**。比如 我们想要在Lua中获取Unity中的一个GameObject，那我们Lua中的代码基本上会写成如下的形式

```lua
local gameObject = CS.UnityEngine.GameObject.Find("name")
```

可以看出，这基本上和我们在C#端的书写方式并没有什么差别，但实际上他们的原理却很不一样。

很明显的一点，lua的数据类型根本无法支持**GameObject**，所以我们引入了**Wrap**。无论使用**XLua**还是**ToLua**，在初始化的过程中必不可少的是将我们可能需要使用到的C#类、方法包装起来，以便在Lua中可以相对应的进行数据传递。当我们在Lua中需要调用C#的方法时，将通过**入栈出栈**的方式将**“数据”**进行传递（**注：这里的数据是经过处理的每个Object的Id**），最终完成Lua与C#的数据交互。

**更多详细的理解可阅读**：[Lua C语言API](https://blog.csdn.net/qq_28820675/article/details/106797087)、[ToLua：逐行分析源码，搞清楚Wrap文件原理](https://blog.csdn.net/qq_28820675/article/details/106864636)



## [LuaCallCSharp]

讲到着，大致是说清楚了我们在配置什么了.... 根据自身的需求将需要在Lua中被调用的东西都配置好。

```csharp
[LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() 
    {
        typeof(Component),
        typeof(Transform),
        typeof(Rigidbody),
        typeof(Behaviour),
        typeof(MonoBehaviour),        
        typeof(GameObject),
        typeof(Time),        
        typeof(Texture),
        typeof(Texture2D),
        typeof(Shader),        
        typeof(Renderer),
        typeof(Screen),        
        typeof(AudioClip),   
        ...
        typeof(自定义...),   
    }
```

## [CSharpCallLua]

​	当然，也可以配置C#直接调用Lua的callback，着一般会配合LuaCallCSharp中的具体方法来使用。具体配置代码XLua中也提供了参考这里代码太长了就不再放出了。



# 初始化

配置说完后，我们就可以针对一开始提及的三行代码进行初始化了。

- `XLua.LuaEnv luaenv = new XLua.LuaEnv();`

  启动**XLua**，创建**LuaEnv**实例，这里面会做**XLua**初始化的工作，其中就包含了Wrap配置中的内容。

- `luaenv.DoString("CS.UnityEngine.Debug.Log('hello world')");`

  加载Lua脚本，**XLua**提供了**DoString()**提供了两种方式：**string/byte[]** ，实际上最终都是变为**byte[]**。

  用例中的该方式比较直接，适合用来做简单的性测使用，一般项目中并不会这么用。

  **XLua**提供了**luaenv.AddLoader(CustomLoader loader)** 方法，让我们自定义加载脚本的加载器。

  `public delegate byte[] CustomLoader(ref string filepath);`

  只需定义一个返回值为 **byte[]** 的方法即可。

- `luaenv.Dispose();`

  **Dispose**就过多介绍了，在关闭的时候当然是需要对其进行释放的~



这样，XLua的接入就已经算是初步完成了。以下是作者框架中的相关代码

```csharp
//初始化
public override void Init()
{
    //创建实例
    mLuaEnv = new LuaEnv();
    //添加加载器
    mLuaEnv.AddLoader(XLuaLoader);
    var enterFile = LuaEnterFile;
    //加载Lua第一个脚本
    mLuaEnv.DoString(XLuaLoader(ref enterFile));
	...
}

//加载器
public byte[] XLuaLoader(ref string fileName)
{	
    //正式的加载交由资源管理模块来进行加载
  #if XLua_VASSET || !UNITY_EDITOR
        if(!fileName.EndsWith(".lua"))
            fileName += ".lua";
    	return VAsset.Instance.LoadScriptFile(fileName);
  #elif UNITY_EDITOR
      //简单粗暴的加载方式:)
        string filePath = string.Empty;
    	var name = fileName.Replace(".lua","");
    	mSearchPaths.ForEach(value=>{
            if(File.Exists(value.Replace("?",name)))
            {
                filePath = value.Replace("?",name);
                return;
            }   
    	});
        if(filePath.IsEmptyOrNull())
            return null;
        return File.ReadAllBytes(filePath);
  #endif
}
```



# 小节

因为作者目前还处于缓慢地撸框架阶段，所以对于**XLua**的使用并没有很深入的了解，也没有淌过XLua的坑，所以目前就不再这里在多说太多啦。就作者来言，对比了**ToLua**，**XLua**我觉得在接入上面会舒服很多。（因为二者我都接入了，因为我希望我的框架**HotScript**部分能满足所有的Lua使用者~）





