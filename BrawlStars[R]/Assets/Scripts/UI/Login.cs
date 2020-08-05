using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
public class Login : MonoBehaviour
{
    public static GameObject loginCanvas;
    public static InputField nameInput;
    public static InputField pwdInput;
    public static GameObject popInfo;
    public static Text popInfoText;


    // Start is called before the first frame update
    void Start()
    {
        loginCanvas = GameObject.Find("LoginCanvas");
        nameInput = GameObject.Find("NameInput").GetComponent<InputField>();
        pwdInput = GameObject.Find("PwdInput").GetComponent<InputField>();
        if (GameManager.user_id > 0)
        {
            Destroy(loginCanvas);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
    public static void Popup(string info)
    {
        popInfo = Resources.Load<GameObject>("Prefabs/PopupInfo");
        GameObject pop = Instantiate(popInfo);
        pop.transform.SetParent(loginCanvas.transform);
        pop.transform.position = loginCanvas.transform.position + new Vector3(150, 150, 0);
        pop.GetComponentInChildren<Text>().text = info;
        Destroy(pop, 2f);
    }
    public static void ExecuteLogin(int code, string username, int money, int lv_s, int lv_b, int lv_t)
    {
        switch (code)
        {
            case -1:
                Debug.Log("wrong password!");// wrong password
                Popup("Wrong Password!");
                break;
            case -2:
                Popup("no such user!"); // no such user
                break;
            case -3:
                Popup("user is already online!");
                // user is already online
                break;
            case -4:
                Popup("user already exists(register error)");
                // user already exists(register error)
                break;
            default:
                GameManager.user_id = code;
                GameManager.user_name = username;
                GameManager.money = money;
                GameManager.lv_s = lv_s;
                GameManager.lv_b = lv_b;
                GameManager.lv_t = lv_t;
                nameInput.text = "";
                pwdInput.text = "";
                GameObject lobbyCanvas = Resources.Load<GameObject>("Prefabs/LobbyCanvas");
                Instantiate(lobbyCanvas);
                Destroy(loginCanvas);
                break;
        }
    }
    public static void ExecuteRegister(int code)
    {
        switch (code)
        {
            case -4:
                Popup("user already exists(register error)");
                // user already exists(register error)
                break;
            default:
                Popup("register successfully");
                nameInput.text = "";
                pwdInput.text = "";
                break;
        }
    }
    public void userLogin()
    {
        string userName = nameInput.text;
        string password = pwdInput.text;
        GameManager.CallNet("login", new object[] { userName, password });
    }
    public void test1Login()
    {
        GameManager.CallNet("login", new object[] { "test1", "163" });
    }
    public void test2Login()
    {
        GameManager.CallNet("login", new object[] { "test2", "163" });
    }
    public void test3Login()
    {
        GameManager.CallNet("login", new object[] { "test3", "163" });
    }
    public void userRegister()
    {
        string userName = nameInput.text;
        string password = pwdInput.text;
        // todo
        GameManager.CallNet("register", new object[] { userName, password });
    }
}
