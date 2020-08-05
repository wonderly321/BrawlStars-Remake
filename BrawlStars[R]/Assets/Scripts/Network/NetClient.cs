using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

public abstract class NetClient 
{
    // 事件处理,当出现网络事件会通过事件回调通知上层
    public enum NetEvent 
    {
        CONNECTSUCC,
        CONNECTFAIL,
        RECVMSG,
        CLOSE,
        ERROR
    }

    abstract protected void HandleNetEvent(NetEvent ev, NetClient netclent);

    // 网络状态
    public enum State 
    {
        CONNECTING,
        CONNECTED,
        CONNECTSUCC,
        CONNECTFAIL,
        STOPPED
    }

    public NetClient() 
    {
        this.state = State.STOPPED;
    }

    public Socket socket;
    public State state;

    // 发送和接收缓冲区
    static int HEAD_LENGTH = 4;
    byte[] recvHead = new byte[HEAD_LENGTH];
    byte[] recvBuf = null;
    byte[] sendBuf = null;
    int recvBufOffset;
    int sendBufOffset;
    int recvBufLen;
    int sendBufLen;

    bool isRecvHead = false;
    bool isConnErr = false;

    List<byte[]> sendLst = new List<byte[]>();
    List<byte[]> recvLst = new List<byte[]>();

	public void Update () 
    {
        // 连接成功
        if (state == State.CONNECTSUCC) 
        {
            state = State.CONNECTED;
            // 接收缓冲区初始化
            isRecvHead = true;
            recvBufOffset = 0;
            recvBufLen = 4;
            recvBuf = recvHead;
            // 发送缓冲区初始化
            sendBuf = null;
            sendBufOffset = sendBufLen = 0;
            // 发送连接成功事件，
            HandleNetEvent(NetEvent.CONNECTSUCC, this);
            // 连接失败
        } 
        else if (state == State.CONNECTFAIL) 
        {
            state = State.STOPPED;
            // 发送连接失败事件
            HandleNetEvent(NetEvent.CONNECTFAIL, this);
            // 处于保持连接状态，每次循环尝试接收新数据以及发送未发送的数据
        } 
        else if (state == State.CONNECTED) 
        {
            tryRecv();
            trySend();
        } 
        else if (state == State.STOPPED) 
        {
            // connect错误
            if (isConnErr) 
            {
                isConnErr = false;
                HandleNetEvent(NetEvent.CONNECTFAIL, this);
            }
        }
	}

    public void Connect(string host, int port) 
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            state = State.CONNECTING;
            socket.Blocking = true;
            socket.BeginConnect(host, port, new AsyncCallback(this.OnConnect), null);
        } 
        catch (SocketException ex)
        {
            Error(ex.ToString());
            isConnErr = true;
            state = State.STOPPED;
        }
    }

    private void OnConnect(IAsyncResult ret) 
    {
        try 
        {
            socket.EndConnect(ret);
            // 修改为非阻塞
            socket.Blocking = false;
            if (socket.Connected) 
            {
                state = State.CONNECTSUCC;
            } 
            else 
            {
                state = State.CONNECTFAIL;
            }
        } 
        catch (Exception ex) 
        {
            Error(ex.ToString());
            isConnErr = true;
            state = State.STOPPED;
        }
    }

    public void Close() {
        state = State.STOPPED;
        try 
        {
            if (socket != null && socket.Connected) 
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        } 
        catch (Exception e) 
        {
            Error(e.ToString());
            // do nothing
        }
        HandleNetEvent(NetEvent.CLOSE, this);
    }

    protected byte[] GetMsg() 
    {
        // 获取一条完整的消息
        byte[] ret = null;
        if (recvLst.Count > 0) 
        {
            ret = recvLst[0];
            recvLst.RemoveAt(0);
        }
        return ret;
    }

    protected void Send(byte[] buf) 
    {
        byte[] lenBytes = NetUtil.IntToBytesLD(buf.Length + HEAD_LENGTH);
        sendLst.Add(lenBytes);
        sendLst.Add(buf);
        trySend();
    }

    private void Error(string msg) 
    {
        UnityEngine.Debug.LogError(msg);
        HandleNetEvent(NetEvent.ERROR, this);
        Close();
    }

    private void trySend() 
    {
        if (sendLst.Count == 0) 
        {
            return;
        }
        
        while (sendLst.Count > 0 && state == State.CONNECTED)
        {
            // 前一个缓冲区发送完毕，开始发送新的缓冲区
            if (sendBuf == null) 
            {
                sendBuf = sendLst[0];
                sendBufOffset = 0;
                sendBufLen = sendBuf.Length;
            }
            int num = 0;
            try 
            {
                num = socket.Send(sendBuf, sendBufOffset, sendBufLen - sendBufOffset, SocketFlags.None);
            } 
            catch (SocketException se) 
            {
                SocketError code = se.SocketErrorCode;
                if (code == SocketError.WouldBlock ||
                    code == SocketError.NoBufferSpaceAvailable ||
                    code == SocketError.IOPending) {
                    break;
                } 
                else 
                {
                    Error(se.ToString());
                    return;
                }
            }
            sendBufOffset += num;
            // 当前缓冲区发送完成
            if (sendBufOffset == sendBufLen) 
            {
                sendBuf = null;
                sendLst.RemoveAt(0);
            }
        }
    }

    private void tryRecv() 
    {
        // 一直读到没数据可读
        while (/* socket.Available > 0 && */ state == State.CONNECTED) 
        {
            int num = 0;
            try 
            {
                num = socket.Receive(recvBuf, recvBufOffset, recvBufLen - recvBufOffset, SocketFlags.None);
            } 
            catch (SocketException se) 
            {
                SocketError code = se.SocketErrorCode;
                if (code == SocketError.WouldBlock ||
                    code == SocketError.NoBufferSpaceAvailable ||
                    code == SocketError.IOPending) 
                {
                    break;
                } 
                else 
                {
                    Error(se.ToString());
                    return;
                }
            }
            // 读到数据
            if (num > 0) 
            {
                recvBufOffset += num;
                // 当前缓冲区已经读满
                if (recvBufOffset == recvBufLen) 
                {
                    if (isRecvHead) 
                    {
                        // 读完了包头，通过包头获取包体的长度，开始读取包体
                        isRecvHead = false;
                        recvBufOffset = 0;
                        recvBufLen = NetUtil.BytesToIntLD(recvBuf, 0) - HEAD_LENGTH;
                        recvBuf = new byte[recvBufLen];
                    } 
                    else 
                    {
                        // 读完了包体，将包放到recvLst中，发送RECVMSG事件并继续读取包头
                        recvLst.Add(recvBuf);
                        HandleNetEvent(NetEvent.RECVMSG, this);
                        isRecvHead = true;
                        recvBufOffset = 0;
                        recvBufLen = HEAD_LENGTH;
                        recvBuf = recvHead;
                    }
                }
            // 连接被关闭
            } 
            else if (num == 0)
            {
                Close();
            }
        }
    }
}
