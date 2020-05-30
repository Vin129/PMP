using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace EasyFramework
{
	using PMP.Extension;
	public class EasyHttp : MonoBehaviour
	{
		#region members
		private EasyPool<WWWRequest> mRequestPool;
		private LinkedList<WWWRequest> mRequestList = new LinkedList<WWWRequest>();
		private Dictionary<string, string> mHeader = new Dictionary<string, string>();
		private string mHeaderCookie = "";
		#endregion
		public static EasyHttp Instance;
		private bool mReachable = false;
		private void Awake() 
		{
			Instance = this;
		}
		public void Init()
		{
			mHeader["Content-Type"] = "application/json";  
			mRequestPool = new EasyPool<WWWRequest>();
			StartCoroutine("RequestProcess");
		}

		public void Update()
		{
			if (Application.internetReachability == NetworkReachability.NotReachable && mReachable) {
				mReachable = false;
				LostConnection();
			}
			if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork && !mReachable)
			{
				mReachable = true;
				SuccessConnection();
			}
			if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && !mReachable)
			{
				mReachable = true;
				SuccessConnection();
			}
		}

		IEnumerator RequestProcess()
		{
			while(true)
			{
				WWWRequest kRequest = null;
				if(mRequestList.Count > 0)
				{
					kRequest = mRequestList.First.Value;
					yield return kRequest.WWWObj;
					mRequestList.RemoveFirst();
					bool bHandled = false;
					if(null != kRequest.WWWObj.responseHeaders && kRequest.WWWObj.responseHeaders.Count > 0)
					{
						if(string.IsNullOrEmpty(kRequest.WWWObj.error))
						{
							WWWHandler(kRequest);
							mRequestPool.Recycle(kRequest);
							bHandled = true;
						}
					}
					if(false == bHandled)
					{
						WWW kWWW = new WWW(kRequest.WWWObj.url);
						kRequest.WWWObj.Dispose();
						kRequest.WWWObj = kWWW;
						kRequest.OnFailed();

						if(kRequest.RetryMaxCount())
						{
							// Process Lost Connection;
							OnLostConnection();
							mRequestList.AddFirst(kRequest);
						}
						else
						{
							//process time out
							OnTimeOut();
						}
					}


					if(0 == mRequestList.Count)
					{
						// Process No Request
						OnNoRequest();
					}

				}
				else
					yield return null;
			}
		}

		// Process Lost Connection
		private void OnLostConnection()
		{
			
		}

		// 超时处理
		private void OnTimeOut()
		{

		}

		//网络从连接到断开
		private void LostConnection() {

		}

		//网络从断开到连接
		private void SuccessConnection() {

		}

		//重新开启协程
		public void RestCoroutine() {
			mRequestList.Clear();
			StopCoroutine("RequestProcess");
			StartCoroutine("RequestProcess");
		}

		private void OnNoRequest()
		{

		}

		private void WWWHandler(WWWRequest kRequest)
		{
			if (null == kRequest || null == kRequest.WWWObj)
				return;
			if (kRequest.WWWObj.responseHeaders.ContainsKey("SET-COOKIE"))
			{
				mHeaderCookie = kRequest.WWWObj.responseHeaders["SET-COOKIE"];
				if (mHeaderCookie.Contains(@"; "))
					mHeaderCookie = mHeaderCookie.Substring(0, mHeaderCookie.IndexOf(@"; "));
				mHeader["Cookie"] = mHeaderCookie;
			}
			if(null != kRequest.CallbackFunc)
			{
				kRequest.CallbackFunc.Invoke(kRequest.WWWObj.text,kRequest.WWWObj.error);
			}
			else
			{

			}
		}

		public virtual void Request(string strURL,string strBody,HttpCallback kFunc)
		{
			if(mHeader.ContainsKey("Cookie"))
				Log.I(mHeader["Cookie"]);
			WWWRequest kRequest = mRequestPool.Get();
			Log.I(strURL);
			kRequest.WWWObj = new WWW(strURL,System.Text.Encoding.UTF8.GetBytes(strBody),mHeader);
			kRequest.CallbackFunc = kFunc;
			mRequestList.AddLast(kRequest);
		}

		public virtual void Request(string strURL,HttpCallback kFunc)
		{
			if(mHeader.ContainsKey("Cookie"))
				Log.I(mHeader["Cookie"]);
			WWWRequest kRequest = mRequestPool.Get();
			kRequest.WWWObj = new WWW(strURL);
			kRequest.CallbackFunc = kFunc;
			mRequestList.AddLast(kRequest);
		}


		private class WWWRequest : IReusable
		{
			public void Reset()
			{
				if(null != mWWW)
					mWWW.Dispose();
				mWWW = null;
				mRetryCount = 0;
			}

			public void Reuse(){}
			public void Recycle(){Reset();}
			public void OnFailed() {++mRetryCount;}
			public bool RetryMaxCount(){return mRetryCount <= mMaxRetryCount;}

			public WWW WWWObj
			{
				get { return mWWW;}
				set { mWWW =value;}
			}

			public HttpCallback CallbackFunc 
			{
				get {return mCallbackFunc;}
				set{mCallbackFunc = value;}
			}

			private WWW mWWW= null;
			private int mRetryCount = 0;//重试的次数
			private int mMaxRetryCount = 3;//最大重试次数
			private HttpCallback mCallbackFunc = null;
		}
	}
}
