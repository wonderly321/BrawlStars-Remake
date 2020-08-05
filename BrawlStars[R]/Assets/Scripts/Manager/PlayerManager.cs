using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static GameObject popBoard;
    public static GameObject playerInfoPrefab;
    public static GameObject mainCanvas;
    public static GameObject resultCanvas;
    public static Button returnButton;
    public static Camera _camera;
    // Start is called before the first frame update
    public static Dictionary<int, GameObject> user2player = new Dictionary<int, GameObject>();
    public static Dictionary<int, string> user2name = new Dictionary<int, string>();
    public static Dictionary<int, GameObject> user2info = new Dictionary<int, GameObject>();
    public static Text remains;
    public static Text[] game_status;
    static bool enableStatusUpdate = false;
    public bool startResultFlag = false;
    void Start()
    {
        mainCanvas = GameObject.Find("MainCanvas");
        GameManager.CallNet("spawn_players", new object[] { GameManager.user_id });
        popBoard = GameObject.Find("PopBoard");
        popBoard.SetActive(false);
        resultCanvas = Resources.Load<GameObject>("Prefabs/ResultCanvas");
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        remains = GameObject.Find("remains").GetComponent<Text>();
        game_status = GameObject.Find("GameStatus").GetComponentsInChildren<Text>();
        enableStatusUpdate = true;
        StartCoroutine(UpdatePlayerStatusCoroutine());
        StartCoroutine(UpdateMyStatusCoroutine());
        playerInfoPrefab = Resources.Load<GameObject>("Prefabs/PlayerInfo");
    }
    void Update()
    {
        if(int.Parse(remains.text) == 1 && GameManager.health > 0 && startResultFlag == false){
            startResultFlag = true;
            GameManager.remains = 0;
            enableStatusUpdate = false;
            var playerInfo = GameObject.Find("PlayerInfo(Clone)");
            playerInfo.SetActive(false);
            popBoard.SetActive(true);
            popBoard.transform.GetComponentsInChildren<Text>()[0].text = "You Win!";
            Button confirmButton = popBoard.GetComponentInChildren<Button>();
            confirmButton.onClick.AddListener(delegate ()
            {
                ConfirmResult();
            });
        }
    }
    IEnumerator UpdatePlayerStatusCoroutine()
    {
        while (enableStatusUpdate)
        {
            UpdatePlayerStatus();
            yield return new WaitForSeconds(0.5f);
        }
    }
    void UpdatePlayerStatus()
    {
        GameManager.CallNet("update_player_status", new object[] { GameManager.room_id });
    }

    IEnumerator UpdateMyStatusCoroutine()
    {
        while (enableStatusUpdate)
        {
            GameManager.CallNet("update_my_status", new object[] { GameManager.user_id });
            yield return new WaitForSeconds(0.5f);
        }
    }

    void LateUpdate()
    {
        UpdateGUIGameStatus();
        UpdateGUIPlayerInfo();
    }

    public void UpdateGUIGameStatus()
    {
        game_status[1].text = GameManager.user_name;
        game_status[3].text = GameManager.energy.ToString();
        //game_status[9].text = GameManager.buff.ToString();
    }

    Vector2 WorldToCanvasPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, position);
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera, out result);
        return canvas.transform.TransformPoint(result);
    }
    
    public void UpdateGUIPlayerInfo()
    {
        var canvas = mainCanvas.GetComponent<Canvas>();
        var canvasRect = mainCanvas.GetComponent<RectTransform>();
        foreach (var user_id in user2info.Keys)
        {
            var info = user2info[user_id];
            info.transform.position = new Vector2(0, 50) + WorldToCanvasPosition(canvas, canvasRect, _camera, user2player[user_id].transform.position);
            if (user_id == GameManager.user_id)
            {
                info.transform.Find("HpSlider").GetComponent<Slider>().value = GameManager.health;
                info.transform.Find("AttackSlider").GetComponent<Slider>().value = GameManager.normal_number;
            }
            else
            {
                info.transform.Find("HpSlider").GetComponent<Slider>().value = user2player[user_id].GetComponent<PlayerController>().health;
            }
        }
    }
    public static void ExecuteBorn(int user_id, string name, string character, int hp, int norm_damage, int comb_damage, int velocity, int norm_number, int energy, int buff, int score, float x, float z)
    {
        var characterPrefab = Resources.Load<GameObject>("Prefabs/" + character);
        var hero = Instantiate(characterPrefab);
        hero.transform.position = new Vector3(x, 0, z);
        user2player[user_id] = hero;
        user2name[user_id] = name;
        var heroController = hero.GetComponent<PlayerController>();
        heroController.lastLogicPosition = new Vector3(x, 0, z);
        heroController.logicPosition = new Vector3(x, 0, z);
        if (user_id == GameManager.user_id)
        {
            hero.tag = "myself";
            GameManager.health = hp;
            GameManager.max_hp = hp;
            GameManager.normal_damage = norm_damage;
            GameManager.combo_damage = comb_damage;
            GameManager.velocity = velocity;
            GameManager.normal_number = norm_number;
            GameManager.energy = energy;
            GameManager.buff = buff;
            GameManager.score = score;
            heroController.moveSpeed = velocity;
        }
        else
        {
            hero.tag = "enermy";
            heroController.user_id = user_id;
            heroController.health = hp;
            heroController.max_hp = hp;
            heroController.buff = buff;
        }
        // GUI playerInfo
        var playerInfo = Instantiate(playerInfoPrefab);
        playerInfo.transform.SetParent(mainCanvas   .transform);
        user2info[user_id] = playerInfo;
        if (user_id == GameManager.user_id)
        {
            playerInfo.transform.Find("HpSlider").GetComponent<Slider>().maxValue = hp;
            playerInfo.transform.Find("AttackSlider").GetComponent<Slider>().maxValue = norm_number;
        }
        else
        {
            playerInfo.transform.Find("HpSlider").GetComponent<Slider>().maxValue = hp;
            playerInfo.transform.Find("AttackSlider").gameObject.SetActive(false);
        }
    }

    public static void ExecuteMove(int user_id, float x, float z)
    {   
        var hero = user2player[user_id];
        var heroController = hero.GetComponent<PlayerController>();
        heroController.ExecuteMove(x, z);
    }
    public static void ExecuteRepel(int user_id, float x, float z)
    {
        if (!user2player.ContainsKey(user_id)) return;
        var hero = user2player[user_id];
        var heroController = hero.GetComponent<PlayerController>();
        heroController.ExecuteRepel(x, z);
    }
    public static void ExecuteNormalAttack(int user_id, float arrow_rot_y)
    {
        var hero = user2player[user_id];
        var heroController = hero.GetComponent<PlayerController>();
        heroController.ExecuteNormalAttack(arrow_rot_y);
    }

    public static void ExecuteComboAttack(int user_id, float x, float y, float z)
    {
        var hero = user2player[user_id];
        var heroController = hero.GetComponent<PlayerController>();
        heroController.ExecuteComboAttack(x, y, z);
    }
    public static void ExecuteDead(int user_id)
    {
        var playerInfo = GameObject.Find("PlayerInfo(Clone)");
        playerInfo.SetActive(false);
        if (!user2player.ContainsKey(user_id)) return;
        var hero = user2player[user_id];
        var heroController = hero.GetComponent<PlayerController>();
        // hero.GetComponent<PlayerController>().isDead = true;
        heroController.ExecuteDead();
        Destroy(user2info[user_id], 1f);
        user2info.Remove(user_id);
        user2player.Remove(user_id);
        if (hero.tag == "myself")
        {
            enableStatusUpdate = false;
            popBoard.SetActive(true);
            Button confirmButton = popBoard.GetComponentInChildren<Button>();
            confirmButton.onClick.AddListener(delegate ()
            {
                ConfirmResult();
            });
            
        }
    }
    public static void ConfirmResult()
    {        
        int money = GameManager.score * 50 + GameManager.buff * 10;
        GameManager.CallNet("update_money", new object[]{GameManager.user_id, GameManager.money + money});
        Instantiate(resultCanvas);
        Destroy(mainCanvas);  
    }
    
    public static void ExecuteUpdatePlayerStatus(
        List<object> uids,
        List<object> hps,
        List<object> max_hps,
        List<object> buff
    )
    {
        int n = uids.Count;
        for (int i = 0; i < n; i++)
        {
            if ((int)uids[i] == GameManager.user_id)
            {
                GameManager.health = (int)hps[i];
                GameManager.max_hp = (int)max_hps[i];
                GameManager.buff = (int)buff[i];
            }
            else
            {
                if (!user2player.ContainsKey((int)uids[i])) continue;
                var heroController = user2player[(int)uids[i]].GetComponent<PlayerController>();
                heroController.health = (int)hps[i];
                heroController.max_hp = (int)max_hps[i];
                heroController.buff = (int)buff[i];
            }
        }
        remains.text = n.ToString();
    }

    public static void ExecuteUpdateMyStatus(
        int normal_number, int energy, int score
    )
    {
        GameManager.normal_number = normal_number;
        GameManager.energy = energy;
        GameManager.score = score;
    }
}
