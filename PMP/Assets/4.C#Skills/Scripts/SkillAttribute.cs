using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
public class SkillAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.Class,AllowMultiple = true)]
public class SimpleCalledClassAttribute:Attribute{
	public SimpleCalledClassAttribute(){

	}
	private Dictionary<string,MethodInfo> FunctionPryInfos = new Dictionary<string, MethodInfo>();
	public void GetNoParamFunctionNames(object o)
	{
		MethodInfo[] infos = o.GetType().GetMethods(BindingFlags.InvokeMethod & BindingFlags.Public);
		infos.Where(info=>(info.ReturnType == typeof(void) && info.GetParameters().Length == 0));
		infos = infos.ToArray();
		foreach (var item in infos)
		{
			FunctionPryInfos.Add(item.Name,item);
		}
	}
	public void Call(object o,string functionName)
	{

	}
}
