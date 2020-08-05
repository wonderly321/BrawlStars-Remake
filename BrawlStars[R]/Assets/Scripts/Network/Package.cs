using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]//序列化
public class Packet
{
	public static byte[] Pack(Packet pkt)
	{
		string jsonStr = MiniJSON.Json.Serialize(pkt.values);
		return System.Text.Encoding.ASCII.GetBytes(jsonStr);
	}

	public static Packet Unpack(byte[] msg)
	{
		string jsonStr = System.Text.Encoding.ASCII.GetString(msg);
		Packet pkt = new Packet();
		pkt.values = MiniJSON.Json.Deserialize(jsonStr) as Dictionary<string, object>;
		return pkt;
	}

	public Packet()
	{
		//
	}

	public Packet(string m, object[] p)
	{
		values = new Dictionary<string, object>();
		values.Add("kwargs", new Dictionary<string, object>());
		values.Add("method", m);
		values.Add("args", p);
	}

	public string Method { get { return values["method"] as string; } }
	public object[] Args { get { return (values["args"] as List<object>).ToArray(); } } 

	private Dictionary<string, object> values = null;
}


