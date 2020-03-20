using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;

using System.IO;
 
public abstract class NetManager : MonoBehaviour
{
	public const int ReconnectWarningMaxCount = 3;

	private int m_reconnectWarningCount = ReconnectWarningMaxCount;
	public int ReconnectWarningCount {
		get { return m_reconnectWarningCount; }
		set { m_reconnectWarningCount = value; }
	}
	

	public delegate void RevCallBack();
	public Dictionary<int, RevCallBack> revCallBackDic = new Dictionary<int, RevCallBack>();
	
	//--------------------------------------------------------------
	public delegate void NetDelegate(bool _connected);
    //--------------------------------------------------------------	

    public static readonly float RECONNECT_INTERVAL = 5;

	//! reconnect attribute
	protected float m_reconnectInterval		= 0;
	
	//! connection state
	protected bool m_connectingState		= false;
	
	//! store the server host and port to reconnect
	protected string m_serverHost = "";
	protected int m_serverPort	= 0;
	
	//! connection is done delegate
	protected NetDelegate	m_connectDoneDelegate = null;

	// Explant the Client class to here
#if !UNITY_EDITOR && UNITY_BLACKBERRY
	protected BBSocket m_socket = new BBSocket();
#else
	protected CSSocket m_socket = new CSSocket();
#endif

	private List<Packet> m_pendingSendQueue = new List<Packet>();

	private int m_reconnectCount = 0;
	private NetDelegate	m_reconnectEvent;

    protected int m_pulseSendCount = 0;

    public void SetReconnectEvent(NetDelegate _event){
		m_reconnectEvent = _event;
	}

    public void SetSocketErrorEvent(CSSocket.SocketErrorClose _event)
    {
        m_socket.socketErrorClose = _event;
    }

    public string GetSocketIP(){
		return m_socket.GetSocketIP();
	}
		

	void OnDestroy(){
		Disconnect();
	}
		
	// Update is called once per frame
	public virtual void Update () {

		if(!m_connectingState){ // is not connecting

			if(!Connected){

				if(m_serverPort > 0){ // server host and port is configured
					
					m_reconnectInterval += Time.deltaTime;
					
					if (m_reconnectInterval >= RECONNECT_INTERVAL){
						m_reconnectInterval = 0;
						ReconnectServer();				
					}
				}
			}
		}

	}

	public void BeginConnect(string host, int port,NetDelegate dele = null)
	{
		Debug.Log(string.Format("Begin Connect {0}:{1}",host,port));

		if(!m_connectingState){
			m_serverHost = host;
			m_serverPort = port;
			
			m_connectDoneDelegate = dele;

			m_connectingState = true;
            StopAllCoroutines();
            StartCoroutine(StartConnect(m_serverHost, m_serverPort,delegate(bool _connected){

				m_connectingState = false;

				ConnectedDoneProc(_connected);
			}));
		}
	}

	// reconnect server 
	protected virtual void ReconnectServer(){
		
		m_socket.Close();
		
		// clear former message queue
		m_pendingSendQueue.Clear();
		
		Debug.Log("[Reconnect:]Request reconnect the server: " + m_serverHost + " port: " + m_serverPort);

		m_connectingState = true;
        StopAllCoroutines();
        StartCoroutine(StartConnect(m_serverHost, m_serverPort,delegate(bool _connected){

            if (!_connected)
                m_socket.Close();
            m_connectingState = false;

			ReconnectDoneProc(_connected);
			ConnectedDoneProc(_connected);
		}));
	}

    //! connected done produce
    private void ConnectedDoneProc(bool _connected)
    {
        if (m_connectDoneDelegate != null)
        {
            m_connectDoneDelegate(_connected);

            if (_connected)
            {
                m_pulseSendCount = 0;
                m_connectDoneDelegate = null;
            }
        }
    }

    //! reconnect done procduce
    //! parent NetManager reconnect done called
    private void ReconnectDoneProc(bool _connected){

		if (_connected){
			m_reconnectCount		= 0;
            //ReconnectWarningCount = ReconnectWarningMaxCount;

            if (m_reconnectEvent != null){
				m_reconnectEvent(true);
			}
			
			Debug.Log("[Reconnect:]Try reconnect game server is  successful.");
			
		}else{
			
			m_reconnectCount++;
            //if (m_reconnectCount >= ReconnectWarningCount)
            //{

                Debug.Log("[Reconnect:]The connect time is too long. please quit game.");
                //ReconnectWarningCount = 0;

                if (m_reconnectEvent != null){
					m_reconnectEvent(false);
				}
            //}

            Debug.Log("[Reconnect:]The connect request is failed.");
		}
		
		
		Debug.Log("[Reconnect:]Try reconnect game server count is " + m_reconnectCount);
	}
		
	public void Disconnect(){

		Debug.Log("NetManager.Disconnect");
        m_connectingState = false;
        m_socket.Close();
		
		// disable reconnect
		m_serverPort = 0;
		m_serverHost = "";
		
		// clear former message queue
		m_pendingSendQueue.Clear();
	}

	protected virtual void GZIPHeaderBrokenProcess(){
		// close the socket
		m_socket.Close();

		// reconnect immediately
		m_reconnectInterval = RECONNECT_INTERVAL;
	}

	//! this is manager is connected
	public bool Connected{
		get{ return m_socket.Connected;}
	}

    private IEnumerator StartConnect(string host, int port, NetDelegate del){		

		yield return StartCoroutine(m_socket.StartConnect(host,port));

		if(m_socket.Connected){
			StopAllCoroutines();

			StartCoroutine(StartSend());
			StartCoroutine(StartReceive());
			
			m_reconnectInterval = 0;
		}
		
		if (null != del){
			del(m_socket.Connected);
		}
		
		yield break;
    }
	
	/// <summary>
	/// Starts the receive the server's network packet
	/// </summary>
	/// <returns>
	/// The receive.
	/// </returns>
    private IEnumerator StartReceive(){
				
		byte[] t_length = new byte[4];
		byte[] t_header = new byte[4];
		
		int tProcessedPacket = 0;
		
        while(Connected){
			
            if (m_socket.Poll(true)){
				yield return StartCoroutine(m_socket.Read(t_length,t_length.Length));
				if(Connected){
				
					yield return StartCoroutine(m_socket.Read(t_header,t_header.Length));
					if(Connected){

						int size = (t_length[3] | t_length[2] << 8 | t_length[1] << 16 | t_length[0] << 24);
						byte[] bytes = new byte[size];
						if(size != 0){
							yield return StartCoroutine(m_socket.Read(bytes,size));	
						}

						if(Connected){
							
							bool t_zip = (t_header[0] & 0x80) == 0x80;
							bool t_lzma = (t_header[0] & 0x40) == 0x40;
							if(t_zip || t_lzma){
								// clear flag
								t_header[0] = (byte)(t_header[0] & 0x3f);
							}
							
							int opcode = (int)(t_header[3] | t_header[2] << 8 | t_header[1] << 16 | t_header[0] << 24);
							if(t_zip || t_lzma) {
								// decompress
								try{
									bytes = t_lzma ? DecompressLZMA(bytes) : DecompressGZip(bytes);
								}catch(System.Exception e){
									Debug.LogError(e.Message + "\n" + e.StackTrace);
									GZIPHeaderBrokenProcess();
								}
							}

							try{
								DistributePacket(opcode,bytes);
							} catch(System.Exception e){
								if(e is System.Reflection.TargetInvocationException){
									System.Reflection.TargetInvocationException ex = (System.Reflection.TargetInvocationException)e;
									e = ex.InnerException;
								}
								Debug.LogError(e.Message + "\n" + e.StackTrace);
							}							
						}
					}
				}			
				
				if(++tProcessedPacket > 5){
					tProcessedPacket = 0;
					yield return null;
				}
				
            }else{
				
				tProcessedPacket = 0;				
				yield return null;
			}
        }
    }
	
	private LuaInterface.LuaFunction m_luaGetOpcodeNameFunc = null;

	private string GetOpcodeName(int _opcode) {
		if(m_luaGetOpcodeNameFunc == null) {
			return string.Empty;
		}
		m_luaGetOpcodeNameFunc.BeginPCall();
		m_luaGetOpcodeNameFunc.Push(_opcode);
		m_luaGetOpcodeNameFunc.PCall();
		string t_res = m_luaGetOpcodeNameFunc.CheckString();
		m_luaGetOpcodeNameFunc.EndPCall();
		return t_res;
	}

	private LuaInterface.LuaFunction m_luaDistributePacketFunc = null;
	
	private void DistributePacket(int _opcode,byte[] _bytes){
        Debug.Log("received socket proto " + GetOpcodeName(_opcode) + " 0x" + _opcode.ToString("X4"));

        if (m_luaDistributePacketFunc == null) {
			// DistributePacket
			return;
		}

		m_luaDistributePacketFunc.BeginPCall();
		m_luaDistributePacketFunc.Push(_opcode);
		m_luaDistributePacketFunc.Push(new LuaInterface.LuaByteBuffer(_bytes));
		m_luaDistributePacketFunc.PCall();
		m_luaDistributePacketFunc.EndPCall();
	}
	
	private static  byte[] m_gzipTmp = new byte[4096];
	public static byte[] DecompressGZip(byte[] _bytes){

		using (MemoryStream msOut = new MemoryStream()){
			using (MemoryStream msIn = new MemoryStream(_bytes)){
				using (GZipInputStream swZip = new GZipInputStream(msIn)){
					int b;
					
					while((b = swZip.Read(m_gzipTmp,0,m_gzipTmp.Length)) != 0){
						msOut.Write(m_gzipTmp,0,b);
						
						if(b < m_gzipTmp.Length){
							break;
						}
					}
					return msOut.ToArray();  
				}
			}
		}
	}	

	private static byte[] m_emptyBytes = new byte[0];
	public static  byte[] CompressGZip(byte[] _bytes){
		if(_bytes.Length == 0){
			return m_emptyBytes;
		}
			
		using (MemoryStream msIn = new MemoryStream()){
			using (GZipOutputStream swZip = new GZipOutputStream(msIn)){
				swZip.Write(_bytes,0,_bytes.Length);

				// close to finish compress
				swZip.Close();
				return  msIn.ToArray();
			}
		}
	}

	public static LuaInterface.LuaByteBuffer LuaDecompressGZip(byte[] _bytes) {
		return new LuaInterface.LuaByteBuffer(DecompressGZip(_bytes));
	}

	public static LuaInterface.LuaByteBuffer LuaCompressGZip(byte[] _bytes) {
		return new LuaInterface.LuaByteBuffer(CompressGZip(_bytes));
	}

	public static byte[] DecompressLZMA(byte[] _bytes){

		using (MemoryStream msOut = new MemoryStream()){
			using (MemoryStream msIn = new MemoryStream(_bytes)){
				SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();

				const int propertiesSize = 5;
				byte[] properties = new byte[propertiesSize];

				if (msIn.Read(properties, 0, propertiesSize) != propertiesSize){
					throw new Exception("input .lzma bytes is too short");
				}

				decoder.SetDecoderProperties(properties);
				decoder.Code(msIn, msOut, -1, -1, null);

				return msOut.ToArray();
			}
		}
	}

    private IEnumerator StartSend(){
		
		while (Connected){
			
			while(m_pendingSendQueue.Count == 0){
				yield return null;
			}
						
			Packet pak = m_pendingSendQueue[0];
			m_pendingSendQueue.RemoveAt(0);
						
			// compose the packet length and header
			//
			byte[] t_bytes 		= CompressGZip(pak.PacketBytes);
			byte[] t_sendBytes	= new byte[t_bytes.Length + 8];
			
			int t_header = (int)pak.PacketHeader;
			
			t_sendBytes[3] = (byte)(t_bytes.Length & 0x000000ff);
			t_sendBytes[2] = (byte)((t_bytes.Length & 0x0000ff00) >> 8);
			t_sendBytes[1] = (byte)((t_bytes.Length & 0x00ff0000) >> 16);
			t_sendBytes[0] = (byte)((t_bytes.Length & 0xff000000) >> 24);
			
			t_sendBytes[7] = (byte)(t_header & 0x000000ff);
			t_sendBytes[6] = (byte)((t_header & 0x0000ff00) >> 8);
			t_sendBytes[5] = (byte)((t_header & 0x00ff0000) >> 16);
			t_sendBytes[4] = (byte)(((t_header & 0xff000000) >> 24) | 0x80);

			Array.Copy(t_bytes,0,t_sendBytes,8,t_bytes.Length);
			
			// send to server
			//
			yield return StartCoroutine(m_socket.Send(t_sendBytes));

            Debug.Log("sended socket proto " + GetOpcodeName(pak.PacketHeader) + " 0x" + pak.PacketHeader.ToString("X4"));
        }
    }
	
	/// <summary>
	/// Sends the packet.
	/// </summary>
	/// <param name='pak'>
	/// Pak.
	/// </param>
    public virtual void SendPacket(Packet pak){
		m_pendingSendQueue.Add(pak);
    }
}


