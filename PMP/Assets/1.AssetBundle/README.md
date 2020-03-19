# AssetBundle

#### 常使用的API

1.用于返回assetObject相对于工程目录的存储路径

```c#
AssetDatabase.GetAssetPath(obj);
```

2.设置AB名（可以手动操作）

```c#
AssetImporter ai.assetBundleName
```

3.生成AB包

```c#
BuildPipeline.BuildAssetBundles
(path,BuildAssetBundleOptions,BuildTarget);
```

4.加载AB包及AB资源

```c#
AssetBundle.LoadFromFile(paht);
AssetBundle.AssetBundle.LoadFromMemory(byte); //适用于加密

AssetBundle.LoadAsset(assetName)
AssetBundle.LoadAllAssets() 加载AB包中所有的对象，不包含依赖的包
AssetBundle.LoadAssetAsync() 异步加载，加载较大资源的时候
AssetBundle.LoadAllAssetsAsync() 异步加载全部资源
AssetBundle.LoadAssetWithSubAssets 加载资源及其子资源

```

5.卸载AB

```c#
AssetBundle.Unload(true) 卸载所有资源，包含其中正被使用的资源
AssetBundle.Unload(false) 卸载所有没被使用的资源
Resources.UnloadUnusedAssets 卸载个别未使用的资源
```







### [BuildAssetBundleOptions](https://blog.csdn.net/AnYuanLzh/article/details/81485762)

| 类型                                    | 含义                                                         |
| :-------------------------------------- | ------------------------------------------------------------ |
| None                                    | 默认                                                         |
| UncompressedAssetBundle                 | 不压缩，体积大，加载快                                       |
| CompleteAssets                          | 强制包括整个资源                                             |
| DisableWriteTypeTree                    | 不包含类型信息。发布web平台时，不能使用该选项                |
| DeterministicAssetBundle                | 使每个object具有唯一不变的hashID, 可用于增量式发布AssetBundle |
| ForceRebuildAssetBundle                 | 强制重新build所有ab                                          |
| IgnoreTypeTreeChanges                   | 忽略typetree的变化，不能与DisableWriteTypeTree同时使用       |
| AppendHashToAssetBundleName             | 附加hash到assetbundle名字中                                  |
| ChunkBasedCompression                   | 使用lz4的格式压缩ab,ab会在加载资源时才进行解压。默认的压缩格式是lzma,它会使用ab在下立即解压。 |
| StrictMode                              | 使用严格模式build ab, 有任何非致命的error都不会build成功.    |
| DryRunBuild                             | Do a dry run build                                           |
| DisableLoadAssetByFileName              | 不使用FileName来加载ab                                       |
| DisableLoadAssetByFileNameWithExtension | 不使用带后缀的文件名来加载ab                                 |



学习链接：https://blog.csdn.net/qq_35361471/article/details/82854560







