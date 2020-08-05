using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;


// 全局游戏管理类
public class GameManager : MonoBehaviour
{
    // 全局变量
    public static bool IsCurLocked = false;
    private static bool inited = false;
    private static GameNetClient netClient = null;

    public static int user_id;
    public static int room_id = 0;
    public static string user_name;
    public static int money;
    public static int lv_s;
    public static int lv_b;
    public static int lv_t;
    public static string character;
    public static bool IsMaster = false;
    public static int health;
    public static int max_hp;
    public static int normal_damage;
    public static int combo_damage;
    public static int velocity;
    public static int normal_number;
    public static int energy;
    public static int buff;
    public static int score;
    public static int remains = 10;

    public static Dictionary<string, int> PlayerStatus = new Dictionary<string,int> {};

    void Awake() 
    {
        // 该gameObject不会再切换场景时被销毁
        if (inited) 
        {
            return;
        }
        inited = true;
        DontDestroyOnLoad (transform.gameObject);

        // 加载本地配置文件,主要是一些客户端的配置信息
        //LocalConfig.LoadCfg();
       // LocalLog.WriteLog("服务器地址" + LocalConfig.Get<string>("svrip") + ":" + LocalConfig.Get<int>("svrport"));

        // 设置网络事件回调(连接错误/关闭)
        Net.GNetCB = this.NetCallback;

        // 向服务器端请求游戏配置数据
        //RemoteConfig.RequestCfg();

        IsCurLocked = false;
    }

    private void Start()
    {
        Net.CallMethod("hello_world_from_client", new object[] { 1, "hello world!" });
    }

    // 保存一些全局的属性，其它类通过GameManager获取这些属性
    public static GameNetClient Net 
    {
        get 
        {
            if (netClient == null) 
            {
                netClient = new GameNetClient();
            }
            return netClient;
        }
    }

    void Update() 
    {
        if (IsCurLocked) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } 
        else 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // 按ESC退出游戏
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                        Application.Quit();
            #endif
        }
        // 收发数据包
        Net.Update ();
	}

    public void NetCallback(int ev) 
    {
        //TODO
    }
    public static void CallNet(string methodName, object[] data)
    {
        // Debug.Log("CallMethod: " + methodName + "  datalength: " + data.Length);
        Net.CallMethod(methodName, data);
    }
    


}
