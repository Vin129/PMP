namespace EasyFramework
{
	using PMP.Extension;
	using System.Collections.Generic;
	using UnityEngine;
	public delegate void NetErrorClose();
	public delegate void  HttpCallback(string resp,string error);
	public delegate void  SocketCallback(IPacket packet);
	public delegate IPacket GetPacket();
	public delegate void RecyclePacket(IPacket packet);
    public class EasyNet : MonoBehaviour
	{
		public static EasyNet Instance;
		private EasyHttp mEasyHttp;
		public EasyHttp Http
		{
			get
			{
				if(mEasyHttp == null)
				{
					if(EasyHttp.Instance == null)
						mEasyHttp = gameObject.AddComponent<EasyHttp>();
					else 
						mEasyHttp = EasyHttp.Instance;
					mEasyHttp.Init();
				}
				return mEasyHttp;
			}
		}

		private void Awake() 
		{
			Instance = this;	
		}
	}
}

