using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject avatar = GameObject.Find("playerAvatar").gameObject;
        avatar.GetComponent<Image>().sprite = Resources.Load("Images/avatar" + GameManager.character as string, typeof(Sprite)) as Sprite;
        var texts = transform.GetComponentsInChildren<Text>();
        texts[1].text = (GameManager.remains + 1).ToString();
        texts[3].text = GameManager.score.ToString();
        texts[5].text = GameManager.buff.ToString();
        texts[7].text = (GameManager.score * 50 + GameManager.buff *10).ToString();
        Button returnButton = transform.GetComponentInChildren<Button>();
        returnButton.onClick.AddListener(delegate ()
        {
            ReturnLobby();
        });
    }
    public static void ReturnLobby()
    {
        GameManager.CallNet("exit_room", new object[]{GameManager.user_id, GameManager.room_id});
        SceneManager.LoadScene("Scenes/welcome");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
