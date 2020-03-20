using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using PMP.Extension;
public class HttpNetworkHandler : MonoBehaviour
{
    private bool mReachable = false;
    private class WWWRequest : IRecycleableObj
    {
        public void Reset()
        {
            if(null != mWWW)
                mWWW.Dispose();
            mWWW = null;
            mRetryCount = 0;
        }

        public void OnReuse(){}
        public void OnRecyle(){Reset();}
        public void OnFailed() {++mRetryCount;}
        public bool RetryMaxCount(){return mRetryCount <= mMaxRetryCount;}

        public WWW WWWObj
        {
            get { return mWWW;}
            set { mWWW =value;}
        }

        public object CallbackFunc 
        {
            get {return mCallbackFunc;}
            set{mCallbackFunc = value;}
        }

        private WWW mWWW= null;
        private int mRetryCount = 0;//重试的次数
        private int mMaxRetryCount = 3;//最大重试次数
        private object mCallbackFunc = null;
    }

    void Start()
    {
        mHeader["Content-Type"] = "application/json";  

        mRequestPool = new Pool<WWWRequest>(5);
        StartCoroutine("RequestProcess");
    }


    private void Update()
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
                        mRequestPool.Recyle(kRequest);
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
        //Debug.LogError("LostConnection");
    }

    //网络从断开到连接
    private void SuccessConnection() {
        //Debug.LogError("SuccessConnection");
    }

    //重新开启协程
    public void RestCoroutine() {
        //Debug.LogError("RestCoroutine");
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
            Log.E(mHeaderCookie);
        }
        if(null != kRequest.CallbackFunc)
        {
            if(kRequest.CallbackFunc is LuaFunction)
            {
                LuaFunction kFunc = kRequest.CallbackFunc as LuaFunction;
                kFunc.Call(kRequest.WWWObj.error,kRequest.WWWObj.text);
            }
        }
        else
        {
            // "Network.OnHttpRequestCb",this,kRequest.WWWObj.error,kRequest.WWWObj.text
        }
    }

    public virtual void Request(string strURL,string strBody,object kFunc )
    {
        if(mHeader.ContainsKey("Cookie"))
            Log.E(mHeader["Cookie"]);
        WWWRequest kRequest = mRequestPool.Get();
        kRequest.WWWObj = new WWW(strURL,System.Text.Encoding.UTF8.GetBytes(strBody),mHeader);
        kRequest.CallbackFunc = kFunc;
        mRequestList.AddLast(kRequest);
    }



    #region members
    private Pool<WWWRequest> mRequestPool;
    private LinkedList<WWWRequest> mRequestList = new LinkedList<WWWRequest>();
    private Dictionary<string, string> mHeader = new Dictionary<string, string>();
    private HttpNetworkHandler mHttpHandler = null;
    private string mHeaderCookie = "";
    #endregion
}
