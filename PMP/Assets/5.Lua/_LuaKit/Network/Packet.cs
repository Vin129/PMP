using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Collections;
using System.IO;

/// <summary>
/// network Packet class for middle layer protobuffer process
/// </summary>
public class Packet{
		
    private int m_header;
    private LuaInterface.LuaByteBuffer m_object;
		
    public Packet(int header,LuaInterface.LuaByteBuffer obj){
        m_header = header;
        m_object = obj;
    }
	
	/// <summary>
	/// Gets the packet header.
	/// </summary>
	/// <value>
	/// The packet header.
	/// </value>
	public int PacketHeader{
		get{return m_header;}
	}

	public byte[] PacketBytes {
		get{return m_object.buffer;}
	}
}