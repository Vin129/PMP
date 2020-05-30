using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyFramework;
using LitJson;
public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var content = new JsonData();
		content["ts"] = 1;
		content["data"] = new JsonData();
		content["data"]["channelId"] = 0;
		content["data"]["version"] = "0.0.1";
		content["data"]["uid"] = "";
		content["data"]["token"] = "";
		EasyNet.Instance.Http.Request("http://update.lcs.legendeleven.com:8080",content.ToString(),(text,error)=>{
			Log.I(text);
		});
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
