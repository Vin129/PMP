using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

public class CSSocket
{
	// Explant the Client class to here
	protected Socket m_socket;
	public CSSocket (){}
	
	private void OnEndConnect(IAsyncResult ar){
		if (null != m_socket) {
			m_socket.EndConnect(ar);
		}
	}
	
	public IEnumerator StartConnect(string host,int port){
		// close former socket first
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
			Debug.LogError ("m_socket.BeginConnect Exception:" + ex.Message);
		}
		
		if(ar != null){
			
			float t_connectingTime = 0;
			
			while (!ar.IsCompleted && t_connectingTime < 5.0f){
				t_connectingTime += Time.deltaTime;				
				yield return null;
			}
		}
	}
	
	public bool Connected{
		get{return m_socket != null && m_socket.Connected;}
	}
	
	public string GetSocketIP(){
		if(Connected){
			return m_socket.AddressFamily.ToString();
		}else{
			return "<NO connection>";
		}
	}

    public delegate void SocketErrorClose();
    public SocketErrorClose socketErrorClose;
    public void Close(bool bError = false){
        if (bError && socketErrorClose != null)
            socketErrorClose();

        try
        {
			
			if (m_socket != null && m_socket.Connected){
				m_socket.Shutdown(SocketShutdown.Both);
				m_socket.Close();
			}
			
		}catch(Exception ex){
			Debug.LogError("m_socket.Shutdown Exception:" + ex.Message);
		}
		
		m_socket = null;
	}
	
	public bool Poll(bool _readOrWrite){
		if(Connected){
			return m_socket.Poll(1, _readOrWrite ? SelectMode.SelectRead : SelectMode.SelectWrite);	
		}
		
		return false;
	}
	
	private const float PollWaitTime = 30;
	
	public IEnumerator Read(byte[] bytes,int len){
		
		int tReadLen = 0;
		float t_time = 0.0f;
		
		while(Connected){
			
			if(Poll(true)){
				try{
					
					tReadLen += m_socket.Receive(bytes,tReadLen,len - tReadLen,SocketFlags.None);
					
					if(tReadLen >= len || t_time > PollWaitTime){
						break;
					}
					
				}catch(Exception e){
                    Debug.LogError(e.ToString());
					Close(true);
					break;
				}	
			}
			
			t_time += Time.deltaTime;
			yield return null;
		}
		
		if(tReadLen < len){
			Close (true);			
		}
	}
	
	public IEnumerator Send(byte[] _bytes){
		
		int t_sendLen = 0;
		float t_time = 0.0f;
		
		while(Connected){
			
			if(Poll(false)){
				
				try{				
					t_sendLen += m_socket.Send(_bytes,t_sendLen,_bytes.Length - t_sendLen,SocketFlags.None);
					
					if(t_sendLen >= _bytes.Length || t_time > PollWaitTime){
						break;
					}
				}catch{
					Close(true);
					break;
				}	
			}
			
			t_time += Time.deltaTime;
			yield return null;
		}
		
		if(t_sendLen < _bytes.Length){
			Close (true);			
		}
	}
}
