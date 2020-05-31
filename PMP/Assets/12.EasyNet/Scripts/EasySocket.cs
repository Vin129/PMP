namespace EasyFramework
{
	using UnityEngine;
	using System.Collections;
	using System;
	using System.Net;
	using System.Net.Sockets;
    using System.Collections.Generic;

    public class EasySocket:MonoBehaviour
	{
		CSSocket mSocket = new CSSocket();
		Queue<IPacket> mSendPacketQueue = new Queue<IPacket>();
		Queue<IPacket> mReceivePacketQueue = new Queue<IPacket>();

		GetPacket mGetPacket;
		RecyclePacket mRecyclePacket;
		public void SetPool(GetPacket gp,RecyclePacket rp)
		{
			mGetPacket = gp;
			mRecyclePacket = rp;
		}

		public bool Connected
		{
			get{ return mSocket.Connected;}
		}

		public void BeginConnect(string host,int port,SocketCallback cb)
		{
			mSocket.SocketErrorClose = ()=>{DisConnect();};
			StartCoroutine(Connect(host,port,cb));
		}

		public void Send(byte[] bytes)
		{
			if(mGetPacket == null)
				return;
			var packet = mGetPacket.Invoke();
			packet.Bytes = bytes;
			mSendPacketQueue.Enqueue(packet);
		}

		public void SetReceive(SocketCallback cb)
		{
			if(mGetPacket == null)
				return;
			var packet = mGetPacket.Invoke();
			packet.ESCallback = cb;
			mReceivePacketQueue.Enqueue(packet);
		}

		public void Close()
		{
			mSocket.Close();
			mReceivePacketQueue.Clear();
			mSendPacketQueue.Clear();
		}

		public void DisConnect()
		{
			
		}



		private IEnumerator Connect(string host,int port,SocketCallback cb)
		{
			yield return StartCoroutine(mSocket.StartConnect(host,port));
			if(mSocket.Connected){
				StopAllCoroutines();
				StartCoroutine(LoopSend());
				StartCoroutine(LoopReceive());
			}
			if (null != cb){
				cb.Invoke(null);
			}
			yield break;
		}

		private IEnumerator LoopSend()
		{
			while (Connected)
			{
				while(mSendPacketQueue.Count == 0)
				{
					yield return null;
				}		
				var pak = mSendPacketQueue.Dequeue();
				yield return StartCoroutine(mSocket.Send(pak));
				if(mRecyclePacket != null)
					mRecyclePacket.Invoke(pak);
			}
		}


		private IEnumerator LoopReceive()
		{
			while (Connected)
			{
				while(mReceivePacketQueue.Count == 0)
				{
					yield return null;
				}		
				var pak = mReceivePacketQueue.Dequeue();
				yield return StartCoroutine(mSocket.Send(pak));
				pak.Execute();
				if(mRecyclePacket != null)
					mRecyclePacket.Invoke(pak);
			}
		}

		private class CSSocket
		{
			protected Socket m_socket;
			public CSSocket(){}
			private void OnEndConnect(IAsyncResult ar)
			{
				if (null != m_socket) 
				{
					m_socket.EndConnect(ar);
				}
			}
			
			public IEnumerator StartConnect(string host,int port){
				Close();
				IPAddress[] t_ips = Dns.GetHostAddresses(host);
				if(Socket.OSSupportsIPv6){
					if(t_ips == null || t_ips.Length == 0){
						yield break;
					}else{
						if (t_ips[0].AddressFamily == AddressFamily.InterNetwork){
							m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						}else if (t_ips[0].AddressFamily == AddressFamily.InterNetworkV6){
							m_socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						}else{
							yield break;
						}
					}
				}else{
					m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				}
				
				// Setup our options:
				// * NoDelay - don't use packet coalescing
				// * DontLinger - don't keep sockets around once they've been disconnected
				m_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
				m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
				
				IAsyncResult ar = null;
				try{
					ar = m_socket.BeginConnect(t_ips, port, OnEndConnect, null);
				}catch(Exception ex){
					Log.E("m_socket.BeginConnect Exception:" + ex.Message);
				}
				
				if(ar != null){
					float t_connectingTime = 0;
					while (!ar.IsCompleted && t_connectingTime < 5.0f)
					{
						t_connectingTime += Time.deltaTime;				
						yield return null;
					}
				}
			}
			
			public bool Connected
			{
				get{return m_socket != null && m_socket.Connected;}
			}
			
			public string GetSocketIP()
			{
				if(Connected)
				{
					return m_socket.AddressFamily.ToString();
				}else
				{
					return "<NO connection>";
				}
			}

			public NetErrorClose SocketErrorClose;
			public void Close(bool bError = false){
				if (bError && SocketErrorClose != null)
					SocketErrorClose();
				try
				{
					if (m_socket != null && m_socket.Connected){
						m_socket.Shutdown(SocketShutdown.Both);
						m_socket.Close();
					}
				}
				catch(Exception ex)
				{
					Log.E("m_socket.Shutdown Exception:" + ex.Message);
				}
				m_socket = null;
			}
			
			public bool Poll(bool readOrWrite){
				if(Connected)
				{
					return m_socket.Poll(1, readOrWrite ? SelectMode.SelectRead : SelectMode.SelectWrite);	
				}
				return false;
			}
			private const float mPollTime = 30;
			public IEnumerator Read(IPacket packet)
			{
				float t_time = 0.0f;
				while(Connected)
				{
					if(Poll(true))
					{
						try{
							m_socket.Receive(packet.Bytes,SocketFlags.None);
							if(t_time > mPollTime)
							{
								break;
							}
						}
						catch(Exception e)
						{
							Log.E(e.ToString());
							Close(true);
							break;
						}	
					}
					t_time += Time.deltaTime;
					yield return null;
				}
			}

			public IEnumerator Read(IPacket packet,int len)
			{
				int tReadLen = 0;
				float t_time = 0.0f;
				while(Connected)
				{
					if(Poll(true))
					{
						try{
							tReadLen += m_socket.Receive(packet.Bytes,tReadLen,len - tReadLen,SocketFlags.None);
							if(tReadLen >= len || t_time > mPollTime)
							{
								break;
							}
						}
						catch(Exception e)
						{
							Log.E(e.ToString());
							Close(true);
							break;
						}	
					}
					t_time += Time.deltaTime;
					yield return null;
				}
				if(tReadLen < len)
				{
					Close (true);			
				}
			}
			
			public IEnumerator Send(IPacket packet){
				int t_sendLen = 0;
				float t_time = 0.0f;
				while(Connected)
				{	
					if(Poll(false))
					{
						try{				
							t_sendLen += m_socket.Send(packet.Bytes,t_sendLen,packet.Bytes.Length - t_sendLen,SocketFlags.None);
							if(t_sendLen >= packet.Bytes.Length || t_time > mPollTime)
							{
								break;
							}
						}
						catch
						{
							Close(true);
							break;
						}	
					}
					t_time += Time.deltaTime;
					yield return null;
				}
				if(t_sendLen < packet.Bytes.Length)
				{
					Close (true);			
				}
			}
		}
	}

	public interface IPacket:IReusable
	{
		byte[] Bytes { get; set; }
		SocketCallback ESCallback{get;set;}
        void Decompress();
		void Compress();
		void Execute();
	}

	public class EPacket:IPacket
	{
		byte[] mbytes;
		SocketCallback mESCallback;
        public byte[] Bytes
        {
            get
            {
                return mbytes;
            }

            set
            {
                mbytes = value;
            }
        }

        public SocketCallback ESCallback
        {
            get
            {
                return mESCallback;
            }

            set
            {
                mESCallback = value;
            }
        }

        public void Compress()
        {

        }

        public void Decompress()
        {

        }

        public void Execute()
        {
        	if(ESCallback != null)
				ESCallback.Invoke(this);
        }

        public void Recycle()
        {
            mbytes = null;
			mESCallback = null;
        }

        public void Reuse()
        {
            mbytes = null;
			mESCallback = null;
        }
    }
}
