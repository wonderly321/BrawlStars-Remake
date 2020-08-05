using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public static GameObject roomCanvas;
    public static Text roomIDText;
    public static GameObject startButton;
    public static List<Image> playerBoardImages = new List<Image>(10);
    public static List<Text> playerBoardText = new List<Text>(10);
    public static bool enablePlayerUpdate = false;

    public static List<Transform> charTrans = new List<Transform>(3);
    public static List<string> Characters = new List<string>(3){"shooter", "tank", "bomber"};
    public int chooseIndex = 0;
    public static GameObject chooseButton;
    public static GameObject exitButton;
    public static GameObject popInfo;
    // Start is called before the first frame update
    void Start()
    {
        roomCanvas = GameObject.Find("RoomCanvas(Clone)");
        roomIDText = GameObject.Find("RoomID").GetComponent<Text>();        
        startButton = GameObject.Find("StartGameButton");
        chooseButton = GameObject.Find("ChooseButton");
        exitButton = GameObject.Find("ExitRoomButton");
        popInfo = GameObject.Find("RoomPopInfo");
        popInfo.SetActive(false);
        startButton.SetActive(GameManager.IsMaster);
        
        enablePlayerUpdate = true;
        StartCoroutine(UpdatePlayerboardCoroutine());

        var playerBoard = GameObject.Find("PlayerBoard").transform;
        for(int i = 0; i < 10; i++) {
            playerBoardImages.Add(playerBoard.Find("Player" + i.ToString()+"/AvatarPlayerDefault").GetComponent<Image>());
            playerBoardText.Add(playerBoard.Find("Player" + i.ToString() + "/Text").GetComponent<Text>());
        }

        charTrans.Add(GameObject.Find("Shooter_Bow01").transform);
        charTrans.Add(GameObject.Find("Tank_TwoHandsSword01").transform);
        charTrans.Add(GameObject.Find("Bomber_MagicWand01").transform);
        
    }
    IEnumerator UpdatePlayerboardCoroutine()
    {
        roomIDText.text = GameManager.room_id.ToString();
        while(enablePlayerUpdate)
        {
            UpdatePlayerboard();
            yield return new WaitForSeconds(0.5f);
        }
    }
    public static void UpdatePlayerboard()
    {
        // Debug.Log("Room Call UpdatePlayerboard");
        GameManager.CallNet("update_player_board", new object[]{GameManager.room_id});
    }
    public static void ExecuteUpdatePlayerboard(List<object> names, List<object> characters, string master_name, int room_id)
    {
        if(!enablePlayerUpdate) return;
        // Debug.Log("ExecuteUpdatePlayerboard: " + names.Count + " " + characters.Count + " " + master_name);
        for(int i = 0; i < names.Count; i++) {
            playerBoardText[i].text = names[i] as string;
            playerBoardImages[i].sprite = Resources.Load("Images/avatar"+ characters[i] as string, typeof(Sprite)) as Sprite;
        }
        if(master_name == GameManager.user_name) {
            startButton.SetActive(true);
        } else {
            startButton.SetActive(false);
        }
    }

    public void _Start()
    {
        popInfo.SetActive(false);
        StartGame();
    }
    public static void StartGame()
    {
        GameManager.CallNet("start_game", new object[]{GameManager.room_id});
    }
    public static void ExecuteStartGame(int code)
    {
        GameManager.IsCurLocked = true;
        if(code < 0)
        {
            popInfo.SetActive(true);
        }
        else{
            enablePlayerUpdate = false;
            SceneManager.LoadScene("Scenes/main");
            Debug.Log("user [" + GameManager.user_id.ToString() + "] has start game at Room [" + GameManager.room_id.ToString() + "]");
        }

            
    }
    
    public void _ExitRoom() // For Button Click
    {
        ExitRoom();
    }
    public static void ExitRoom()
    {
        GameManager.CallNet("exit_room", new object[]{GameManager.user_id, GameManager.room_id});
    }
    public static void ExecuteExitRoom()
    {
        GameManager.IsCurLocked = false;
        GameManager.room_id = 0;
        enablePlayerUpdate = false;
        GameObject lobbyCanvas = Resources.Load<GameObject>("Prefabs/LobbyCanvas");
        Instantiate(lobbyCanvas);
        Destroy(roomCanvas);
    }

    public void _Choose() // For Button Choose
    {
        Choose(Characters[chooseIndex]);
    }
    public static void Choose(string character)
    {
        GameManager.CallNet("choose_character", new object[]{GameManager.user_id, character});
    }
    public static void ExecuteChoose(string character)
    {
        GameManager.character = character;
        chooseButton.GetComponent<Button>().interactable = false;
        exitButton.GetComponent<Button>().interactable = false;
        Debug.Log("Choosed character [" + character + ']');
    }
    public void _Next()
    {
        chooseIndex = chooseIndex + 1;
        chooseIndex = chooseIndex % 3;
        var tmp = charTrans[2].position;
        for(int i = charTrans.Count - 1; i >= 1; i--) {
            charTrans[i].position = charTrans[i-1].position;
        }
        charTrans[0].position = tmp;
    }
    public void _Prev()
    {
        chooseIndex = chooseIndex - 1 + 3;
        chooseIndex = chooseIndex % 3;
        var tmp = charTrans[0].position;
        for(int i = 0; i < charTrans.Count - 1; i++) {
            charTrans[i].position = charTrans[i+1].position;
        }
        charTrans[2].position = tmp;
    }
}
