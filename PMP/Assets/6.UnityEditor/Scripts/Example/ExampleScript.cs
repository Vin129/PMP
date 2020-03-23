using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PMP.Extension;

[QuickExecute(true)]
public class ExampleScript {

	[ExecuteMethod]
	public void DoExample()
	{
		Log.I("Hellow World");
	}
}



[QuickExecute(true)]
public class TestA {

	[ExecuteMethod]
	public void DoA()
	{
		Log.I("Hellow World");
	}

	[ExecuteMethod]
	public void DoAA(int a)
	{
		Log.I("Hellow World");
	}

	[ExecuteMethod]
	public void DoAAA(int b,bool c)
	{
		Log.I("Hellow World");
	}
}


[QuickExecute(true)]
public class TestB {

	[ExecuteMethod]
	public void DoB(int[] b, params bool[] bools )
	{
		Log.I("Hellow World");
	}
}
