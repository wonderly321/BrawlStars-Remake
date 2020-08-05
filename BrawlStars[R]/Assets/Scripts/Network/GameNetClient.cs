using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class GameNetClient : NetClient 
{
    // 基于通用的NetClient进行拓展
	public GameNetClient() : base() {}

    public static int GNET_CLOSED = 0;
    public static int GNET_CONNFAIL = 1;
    public static int GNET_CONNSUCC = 2;

    // 网络事件回调
    public delegate void GNetCallback(int reason);
    private GNetCallback gnetCb = null;
    public GNetCallback GNetCB 
    {
        set { gnetCb = value; }
    }

    private List<Packet> waitPkt = new List<Packet>();
    public void CallMethod(string method, object[] args) 
    {
        Packet pkt = new Packet(method, args);
        if (this.state != State.CONNECTED) 
        {
            // 发包函数，如果不处于连接状态，会在连接成功后发送
            waitPkt.Add(pkt);
            string host = "127.0.0.1";//LocalConfig.Get<string>("svrip");
            int port = 2000;//LocalConfig.Get<int>("svrport");
            Connect(host, port);
        } 
        else
        {
            // 直接发送
            Send(Packet.Pack(pkt));
        }
    }

	// 处理各种网络事件
	protected override void HandleNetEvent(NetEvent ev, NetClient netclient) 
    {
		if (ev == NetClient.NetEvent.CONNECTSUCC) 
        {
            // 连接服务器成功,发送等待队列中的包
            if (waitPkt != null) 
            {
                foreach (var pkt in waitPkt) 
                {
                    Send(Packet.Pack(pkt));
                }
                waitPkt.Clear();
            }
            if (gnetCb != null) 
            {
                gnetCb(GNET_CONNSUCC);
            }
        } 
        else if (ev == NetClient.NetEvent.CONNECTFAIL) 
        {
			// 连接服务器失败
			if (gnetCb != null) 
            {
                gnetCb(GNET_CONNFAIL);
			}
		} 
        else if (ev == NetClient.NetEvent.CLOSE) 
        {
			// 连接关闭
			if (gnetCb != null) 
            {
                gnetCb(GNET_CLOSED);
			}
		} 
        else if (ev == NetClient.NetEvent.ERROR) 
        {
			// 连接错误,下层会接着发出Close事件，这里不用处理
		} 
        else if (ev == NetClient.NetEvent.RECVMSG) 
        {
			// 收到数据包，分发给对应的Service处理
			Packet pkt = Packet.Unpack(GetMsg());
			if (pkt != null) 
            {
                Dispatcher.Dispatch(pkt);
			}
		}
	}
}
