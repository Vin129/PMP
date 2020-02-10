using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleBuilder : EditorWindow 
{
	// [MenuItem("PMP/AssetBundleBuilder")]
	// private static void ShowWindow() {
	// 	var window = GetWindow<AssetBundleBuilder>();
	// 	window.titleContent = new GUIContent("AssetBundleBuilder");
	// 	window.Show();
	// }

	private void OnGUI() {
		
	}


	[MenuItem ("Assets/1.AssetBundle/SetABName")]
	public static void SetABName()
	{
		//API:AssetDatabase.GetAssetPath
	  	var path = GetSelectedPathOrFallback();
		Debug.Log("相对路径："+ path);
		if (!string.IsNullOrEmpty(path))
		{
			var ai = AssetImporter.GetAtPath(path);
			var dir = new DirectoryInfo(path);
			ai.assetBundleName = dir.Name.Replace(".", "_");
		}
		AssetDatabase.Refresh();
	}

	[MenuItem ("PMP/1.AssetBundle/BuildAB")]
	public static void Build(){
		var abDir = Application.streamingAssetsPath + "/AssetBundles";
		if(!Directory.Exists(abDir))
		{
			Directory.CreateDirectory(abDir);
		}
		BuildPipeline.BuildAssetBundles(abDir,BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);
		AssetDatabase.Refresh();		
	}

	public static string GetSelectedPathOrFallback()
	{
		var path = string.Empty;

		foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);

			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				return path;
			}
		}

		return path;
	}
	
	//弃用 BuildPipeline.BuildAssetBundle
	// [MenuItem("Assets/1.AssetBundle/BuildSelectAB")]
	// public static void BuildSelectAB()
	// {
	// 	var abDir = Application.streamingAssetsPath + "/AssetBundles";
	// 	if(!Directory.Exists(abDir))
	// 	{
	// 		Directory.CreateDirectory(abDir);
	// 	}
	// 	foreach (var o in Selection.GetFiltered(typeof(Object),SelectionMode.Assets))
	// 	{
	// 		var path = AssetDatabase.GetAssetPath(o); 
	// 		Debug.Log("相对路径："+ path);
	// 		DirectoryInfo directoryInfo = new DirectoryInfo(path);
    // 		FileInfo[] fileInfos = directoryInfo.GetFiles("*",SearchOption.AllDirectories);
	// 		List<Object> objs = new List<Object>();
	// 		for(int i = 0;i < fileInfos.Length;i++){
	// 			if (!fileInfos[i].Name.EndsWith(".meta"))
	// 			{
	// 				objs.Add(AssetDatabase.LoadAssetAtPath<Object>(fileInfos[i].FullName));
	// 				Debug.Log("Add"+ fileInfos[i].FullName);
	// 			}
	// 		}
	// 		BuildPipeline.BuildAssetBundle(o,objs.ToArray(),abDir + "/" + o.name.ToLower(),BuildAssetBundleOptions.ChunkBasedCompression,BuildTarget.StandaloneWindows64);
	// 	}
	// 	AssetDatabase.Refresh();
	// }

}
