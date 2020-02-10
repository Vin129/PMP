using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour {
	private string abDir;
	// Use this for initialization
	void Start () {
		abDir = Application.streamingAssetsPath + "/AssetBundles";
		AssetBundle ab = AssetBundle.LoadFromFile(abDir +"/texture");
		var sp = ab.LoadAsset<Sprite>("timg");
		GetComponent<Image>().sprite = sp;
		ab.Unload(false);

		AssetBundle cubeAb = AssetBundle.LoadFromMemory(File.ReadAllBytes(abDir +"/cube"));
		var cube1 = Instantiate(cubeAb.LoadAsset<GameObject>("cube1"));
		cube1.transform.SetParent(this.transform,true);
		cubeAb.Unload(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
