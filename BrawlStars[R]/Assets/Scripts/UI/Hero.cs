using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hero : MonoBehaviour
{
    public static GameObject heroCanvas;
    public static Text money;
    public static GameObject playerAvatar;
    public static GameObject playerName;
    public static Text[] HeroInfo;
    public static Button returnButton, upgradeButton, prev, next;
    public static int chooseIndex = 0;
    public static List<string> playerList = new List<string>{"shooter", "bomber", "tank"};
    public static List<int> playerBaseHealth = new List<int>{3500, 2500, 5000};
    public static List<int> playerBaseNormalDamage = new List<int>{300, 600, 400};
    public static List<int> playerBaseComboDamage = new List<int>{600, 1200, 800};
    public static List<int> playerBaseVelocity = new List<int>{20, 15, 25};
    public static List<int> lv_list = new List<int>{GameManager.lv_s, GameManager.lv_b, GameManager.lv_t};
    public static GameObject popInfo;


    // Start is called before the first frame update
    void Start()
    {
        heroCanvas = GameObject.Find("HeroCanvas(Clone)");  
        HeroInfo = GameObject.Find("HeroInfo").GetComponentsInChildren<Text>();
        money = GameObject.Find("Money").GetComponentInChildren<Text>();
        money.text = (GameManager.money).ToString();
        playerAvatar = GameObject.Find("playerAvatar");
        playerName = GameObject.Find("playerName");
        returnButton = GameObject.Find("Return").GetComponent<Button>();
        upgradeButton = GameObject.Find("Upgrade").GetComponent<Button>();
        prev = GameObject.Find("Prev").GetComponent<Button>();
        next = GameObject.Find("Next").GetComponent<Button>();
        HeroInfo[1].text = (GameManager.lv_s).ToString();
        HeroInfo[3].text = (GameManager.lv_s * 100 + playerBaseHealth[chooseIndex]).ToString();
        HeroInfo[5].text = (GameManager.lv_s * 50 + playerBaseNormalDamage[chooseIndex]).ToString();
        HeroInfo[7].text = (GameManager.lv_s * 50 + playerBaseComboDamage[chooseIndex]).ToString();
        HeroInfo[9].text = (GameManager.lv_s * 1 + playerBaseVelocity[chooseIndex]).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReturnLobby(){
        var lobbyCanvas = Resources.Load("Prefabs/LobbyCanvas");
        Instantiate(lobbyCanvas);
        Destroy(heroCanvas);
    }
    public void Next()
    {
        chooseIndex = chooseIndex + 1;
        chooseIndex = chooseIndex % 3;
        string name = playerList[chooseIndex];
        playerAvatar.GetComponent<Image>().sprite = Resources.Load("Images/avatar" + name, typeof(Sprite)) as Sprite;
        playerName.GetComponentInChildren<Text>().text = name;
        HeroInfo[1].text = (lv_list[chooseIndex]).ToString();
        HeroInfo[3].text = (lv_list[chooseIndex] * 100 + playerBaseHealth[chooseIndex]).ToString();
        HeroInfo[5].text = (lv_list[chooseIndex] * 50 + playerBaseNormalDamage[chooseIndex]).ToString();
        HeroInfo[7].text = (lv_list[chooseIndex] * 50 + playerBaseComboDamage[chooseIndex]).ToString();
        HeroInfo[9].text = (lv_list[chooseIndex] * 1 + playerBaseVelocity[chooseIndex]).ToString();
    }
    public void Prev()
    {
        chooseIndex = chooseIndex - 1 + 3;
        chooseIndex = chooseIndex % 3;
        string name = playerList[chooseIndex];
        playerAvatar.GetComponent<Image>().sprite = Resources.Load("Images/avatar" + name, typeof(Sprite)) as Sprite;
        playerName.GetComponentInChildren<Text>().text = name;
        HeroInfo[1].text = (lv_list[chooseIndex]).ToString();
        HeroInfo[3].text = (lv_list[chooseIndex] * 100 + playerBaseHealth[chooseIndex]).ToString();
        HeroInfo[5].text = (lv_list[chooseIndex] * 50 + playerBaseNormalDamage[chooseIndex]).ToString();
        HeroInfo[7].text = (lv_list[chooseIndex] * 50 + playerBaseComboDamage[chooseIndex]).ToString();
        HeroInfo[9].text = (lv_list[chooseIndex] * 1 + playerBaseVelocity[chooseIndex]).ToString();

    }
    public static void PopUp(string info)
    {
        popInfo = Resources.Load<GameObject>("Prefabs/PopupInfo");
        GameObject pop = Instantiate(popInfo);
        pop.transform.SetParent(heroCanvas.transform);
        pop.transform.position = heroCanvas.transform.position + new Vector3(150, 150, 0);
        pop.GetComponentInChildren<Text>().text = info;
        Destroy(pop, 2f);
    }
    public void Upgrade()
    {
        if(int.Parse(money.text) < 50)
        {
            PopUp("you neew more golds!");
        }
        else{
            next.interactable = false;
            prev.interactable = false;
            upgradeButton.interactable = false;
            string char_name = playerName.GetComponentInChildren<Text>().text;
            int uid = GameManager.user_id;
            GameManager.CallNet("upgrade_character", new object[]{uid, char_name});
        }
        
    }
    public static void ExecuteUpgrade(int lv, int m)
    {
        string c = playerName.GetComponentInChildren<Text>().text;
        if (c == "shooter")
        {
            GameManager.lv_s = lv;
        }
        else if(c == "bomber")
        {
            GameManager.lv_b = lv;
        }
        else{
            GameManager.lv_t = lv;
        }
        money = GameObject.Find("Money").GetComponentInChildren<Text>();
        money.text = m.ToString();        
        HeroInfo[1].text = lv.ToString();
        HeroInfo[3].text = (lv * 100 + playerBaseHealth[chooseIndex]).ToString();
        HeroInfo[5].text = (lv * 50 + playerBaseNormalDamage[chooseIndex]).ToString();
        HeroInfo[7].text = (lv * 50 + playerBaseComboDamage[chooseIndex]).ToString();
        HeroInfo[9].text = (lv * 1 + playerBaseVelocity[chooseIndex]).ToString();
        next.interactable = true;
        prev.interactable = true;
        upgradeButton.interactable = true;
    }
}
