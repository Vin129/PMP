using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GSNetManager : NetManager 
{
	// Heartbeat
	public static readonly int HB_SEND_FAIL_LIMIT = 1;
	
	// Unit is second
	public static readonly float HB_SEND_INTERVAL = 30.0f;
    public static readonly float HEART_IGONRE_INTERVAL_IG = 1f;

    private float mLastSendHeartTime = 0f;

	private LuaInterface.LuaFunction m_luaSendHeartBeatFunc = null;
	private LuaInterface.LuaFunction m_luaReconnectFunc = null;
    private float m_pulseIntervalTimer = 0;

	public void Awake()
    {
        SetSocketErrorEvent(SocketError);
        SetReconnectEvent(ReconnectEvt);
    }

    //ǰ��̨�л�
    void OnApplicationFocus(bool isFocus)
    {
        if (isFocus)
        {
            if (Connected)
            {
                m_pulseIntervalTimer = HB_SEND_INTERVAL;
            }
        }
        else
        {

        }
    }

    public void RequestHeartBeat(){
        if ((m_pulseSendCount == HB_SEND_FAIL_LIMIT) && (Time.time - mLastSendHeartTime) < HEART_IGONRE_INTERVAL_IG) {
            return;
        }
        
        StopCoroutine("heartBeatNotReceive");
        // Confirm the net connect according to the send count.
        if (m_pulseSendCount == HB_SEND_FAIL_LIMIT)
        {
            m_pulseSendCount++;
            Debug.Log("[HeartBeat:]The net connect is lost. will try reconnect the net.");
            ReconnectServer();
        }
        else if (m_pulseSendCount > HB_SEND_FAIL_LIMIT)
        {
            Debug.LogWarning("not SendHeartBeat" + m_pulseSendCount);
        }
        else
        {
            Debug.LogWarning("SendHeartBeat");
            m_pulseSendCount++;
            SendHeartBeat();
            mLastSendHeartTime = Time.time;
            StartCoroutine("heartBeatNotReceive");
        }
    }
    
    IEnumerator heartBeatNotReceive()
    {
        yield return new WaitForSeconds(5);
        RequestHeartBeat();
    }

	private void SendHeartBeat() {
		if(m_luaSendHeartBeatFunc == null) {
            // SendHeartBeat
			return;
		}

		m_luaSendHeartBeatFunc.BeginPCall();
		m_luaSendHeartBeatFunc.PCall();
		m_luaSendHeartBeatFunc.EndPCall();
	}

    private void SocketError()
    {
        RequestHeartBeat();
    }

	private void ReconnectEvt(bool _connected) {
        if (_connected)
            m_pulseSendCount = 0;

        if (m_luaReconnectFunc == null) {
			// ReconnectServer
            return;
		}

		m_luaReconnectFunc.BeginPCall();
		m_luaReconnectFunc.Push(_connected);
		m_luaReconnectFunc.PCall();
		m_luaReconnectFunc.EndPCall();
	}

	// called by Main.lua
	public void ReceiveHeartBeat()
    {
        Debug.LogWarning("ReceiveHeartBeat");
        m_pulseSendCount = 0;
        StopCoroutine("heartBeatNotReceive");
    }
	
	protected override void ReconnectServer(){
		base.ReconnectServer();
		
		// Reset the heart beat send count.
		//m_pulseSendCount = 0;
	}

	public override void Update () {
		
		base.Update();
			
		if (Connected){

            //Send heart beat packet in a fixed time period .
            if ((m_pulseIntervalTimer += Time.deltaTime) >= HB_SEND_INTERVAL)
                {
                    m_pulseIntervalTimer = 0;
                    RequestHeartBeat();
                }
        }
	}

	public override void SendPacket(Packet pak) {
		base.SendPacket(pak);

		if(!Connected) {
            if (m_pulseSendCount < HB_SEND_FAIL_LIMIT)
            {
                mLastSendHeartTime = 0;
                m_pulseSendCount = HB_SEND_FAIL_LIMIT;
            }
            //ReconnectWarningCount = 1;
            RequestHeartBeat();			
		}
	}

}