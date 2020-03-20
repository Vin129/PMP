using System.Collections;
using System.Collections.Generic;
namespace PMP.Extension{
	public static class Log {
		public static void I(string message, params object[] args)
		{
			UnityEngine.Debug.LogFormat(message,args);
		}
		public static void W(string message, params object[] args)
		{
			UnityEngine.Debug.LogWarningFormat(message,args);
		}

		public static void E(string message, params object[] args)
		{
			UnityEngine.Debug.LogErrorFormat(message,args);
		}

		public static void LogInfo<T>(this T obj,params object[] args)
		{
			UnityEngine.Debug.LogFormat(obj.ToString(),args);
		}
	}
}
