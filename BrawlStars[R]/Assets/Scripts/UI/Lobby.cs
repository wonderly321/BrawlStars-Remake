using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby: MonoBehaviour
{ 
    public static GameObject lobbyCanvas;
    public static RectTransform content;
    public static Transform pivot;
    public static GameObject roomInfo;
    public static List<string> waitingRoomList = new List<string>();
    public static Text moneyNumber;
    public static Text user_id;
    public static Text user_name;

    private static bool enableRoomUpdate = false;

    // Start is called before the first frame update
    void Start()
    {
        lobbyCanvas = GameObject.Find("LobbyCanvas(Clone)");
        content = GameObject.Find("Content").GetComponent<RectTransform>();
        pivot = GameObject.Find("Pivot").transform;
        moneyNumber = GameObject.Find("number").GetComponent<Text>();
        user_id = GameObject.Find("userID").GetComponent<Text>();
        user_name = GameObject.Find("userName").GetComponent<Text>();
        user_id.text = GameManager.user_id.ToString();
        user_name.text = GameManager.user_name.ToString();
        moneyNumber.text = GameManager.money.ToString();
        roomInfo = Resources.Load<GameObject>("Prefabs/RoomInfo");
        enableRoomUpdate = true;
        StartCoroutine(UpdateRoomboardCoroutine());
    }
    void Update()
    {
        moneyNumber.text = GameManager.money.ToString();
    }
    IEnumerator UpdateRoomboardCoroutine()
    {
        while(enableRoomUpdate)
        {
            UpdateRoomboard();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void CheckHeros()
    {
        var heroCanvas = Resources.Load("Prefabs/HeroCanvas");
        Instantiate(heroCanvas);
        Destroy(lobbyCanvas);
    }
    public void _CreateRoom()
    {
        CreateRoom();
    }
    public static void CreateRoom()
    {
        GameManager.IsMaster = true;
        GameManager.CallNet("create_room", new object[]{GameManager.user_id});
    }
    
    public static void JoinRoom(int room_id)
    {
        GameManager.CallNet("join_room", new object[]{GameManager.user_id, room_id});
    }
    public static void ExecuteJoin(int room_id)
    {
        GameManager.room_id = room_id;
        enableRoomUpdate = false;
        var roomCanvas = Resources.Load("Prefabs/RoomCanvas");
        Instantiate(roomCanvas);
        Destroy(lobbyCanvas);
        Debug.Log("[" + GameManager.user_id.ToString() + "] join room [ "+ room_id.ToString() +"] succeed");
    }
    public static void UpdateRoomboard()
    {
        // Debug.Log("Lobby Call UpdateRoomboard");
        GameManager.CallNet("update_room_board", new object[]{});
    }
    public static void ExecuteUpdateRoomboard(int rooms, int waits, Dictionary<string, object> waiting_rooms, Dictionary<string, object> gaming_rooms)
    {
        if(!enableRoomUpdate) return;
        waitingRoomList.Clear();
        foreach (Transform child in pivot)
        {
            if(child.gameObject != null){
                Destroy(child.gameObject);
            }            
        }
        content.offsetMin = new Vector2 (content.offsetMin.x, -50 * rooms);
        int index = 0;
        foreach (var oneroom in waiting_rooms)
        { 
            var Room = Instantiate(roomInfo);
            Room.transform.SetParent(pivot);
            Room.transform.position = pivot.position + new Vector3(250, -25 - index*50, 0);
            Text[] texts = Room.GetComponentsInChildren<Text>();
            texts[1].text = oneroom.Key;
            waitingRoomList.Add(oneroom.Key);
            int players = (int)oneroom.Value;
            texts[3].text = players.ToString() + "/10";
            Button button = Room.GetComponentInChildren<Button>();
            
            button.onClick.AddListener(delegate ()
            {
                JoinRoom(int.Parse(oneroom.Key));
            });
            index ++;
        }
        foreach (var oneroom in gaming_rooms)
        {
            var Room = Instantiate(roomInfo);
            Room.transform.SetParent(pivot);
            Room.transform.position = pivot.position + new Vector3(250, -25 - index*50, 0);
            Text[] texts = Room.GetComponentsInChildren<Text>();
            texts[1].text = oneroom.Key.ToString();
            int players = (int)oneroom.Value;
            texts[3].text = players + "/10";
            GameObject button = Room.transform.Find("Button").gameObject;
            button.SetActive(false);
            index ++;
        }       
    }
}
