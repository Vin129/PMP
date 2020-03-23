using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PMP.Extension{
	public static class Log {
		#region Base
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
		# endregion


		#region  Extension
		public static void ErrorReport(this Exception e)
		{
			if(e == null)
				return;
			var message = e.Message + "\n" + e.StackTrace.Split('\n').FirstOrDefault();
			I(message);
		}

		public static void LogInfo<T>(this T obj,params object[] args)
		{
			UnityEngine.Debug.LogFormat(obj.ToString(),args);
		}

		#endregion
	}
}
