using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Dispatcher 
{
	public static void Dispatch(Packet pkt) 
	{
        
		var methodInfo = typeof(Dispatcher).GetMethod(pkt.Method);
        Debug.Log(pkt.Method);
		methodInfo.Invoke(null, pkt.Args);
	}

	public static void recv_msg_from_server(int stat, string msg)
	{
		Debug.Log(string.Format("recv_msg_from_server({0},{1})", stat, msg));
	}

	public static void handle_login_response(int uid, string name, int money, int lv_s, int lv_b, int lv_t)
    {
        Debug.Log("Login Response: " + uid);
        Login.ExecuteLogin(uid, name, money, lv_s, lv_b, lv_t);
    }

    public static void handle_register_response(int code)
    {
        Debug.Log("Register Response: " + code);
        Login.ExecuteRegister(code);
    }
    public static void handle_join_room_response(int user_id, int room_id)
    {
        if (user_id < 0)
        {
            Debug.Log("[" + user_id.ToString() + "] join room [ "+room_id.ToString() +"] failed");
        }
        else {
            Lobby.ExecuteJoin(room_id);    
        }
    }
    public static void handle_update_roomboard_response(int rooms, int waits, Dictionary<string, object> waiting_rooms, Dictionary<string, object> gaming_rooms)
    {
        // Debug.Log("Response Update RoomBoard: " + rooms);
        Lobby.ExecuteUpdateRoomboard(rooms, waits, waiting_rooms, gaming_rooms);
    }
    public static void handle_exit_room_response(int code)
    {
        if (code < 0)
        {
            Debug.Log("Exit Room Failed");
        }
        else{
            Debug.Log("Response Exit Room " + code.ToString());
            Room.ExecuteExitRoom();
        }
    }

    public static void handle_update_playerboard_response(List<object> names, List<object> characters, string is_masters, int room_id)
    {
        // Debug.Log("Resp Update playerboard: " + names.ToString());
        Room.ExecuteUpdatePlayerboard(names, characters, is_masters, room_id);
    }

    public static void handle_choose_character_response(string character)
    {
        Room.ExecuteChoose(character);
    }

    public static void handle_start_game_response(int code)
    {
        Room.ExecuteStartGame(code);
    }

    // Inside Room status
    public static void handle_move_to_response(int user_id, float x, float z)
    {
        
        PlayerManager.ExecuteMove(user_id, x, z); 
        // Debug.Log("user "+ user_id.ToString() + " move to (" + x.ToString() + "," + z.ToString() + ")");
    }
    public static void handle_repel_response(int user_id, float x, float z)
    {
        
        PlayerManager.ExecuteRepel(user_id, x, z); 
        // Debug.Log("user "+ user_id.ToString() + " move to (" + x.ToString() + "," + z.ToString() + ")");
    }
    public static void handle_born_at_response(int user_id, string name, string character, int health, int normal_damage, int combo_damage, int velocity, int normal_number, int energy, int buff, int score, float x, float z)
    {
        
        PlayerManager.ExecuteBorn(user_id, name, character, health, normal_damage, combo_damage, velocity, normal_number, energy, buff, score, x, z);

        Debug.Log("[" + name + "] born at (" + x.ToString()+ ", " + z.ToString() + ")");
    }
    public static void handle_normal_attack_response(int user_id, float arrow_rot_y)
    {
        PlayerManager.ExecuteNormalAttack(user_id, arrow_rot_y);
        Debug.Log("user [" + user_id.ToString() + "] normal attack at (" + arrow_rot_y.ToString() + ")");
    }
    public static void handle_combo_attack_response(int user_id, float x, float y, float z)
    { 
        PlayerManager.ExecuteComboAttack(user_id, x, y, z);        
        Debug.Log("user [" + user_id.ToString() + "] normal attack at (" + x.ToString() + ", " + y.ToString() +", " + z.ToString() + ")");
    }
    public static void handle_dead_response(int hurt_id)
    {
        PlayerManager.ExecuteDead(hurt_id);   
    }
    public static void handle_update_player_status_response(List<object> uids, List<object> hps, List<object> max_hps, List<object> buff)
    {
        PlayerManager.ExecuteUpdatePlayerStatus(uids, hps, max_hps, buff);
    }

    public static void handle_update_my_status_response(int normal_number, int energy, int score)
    {
        PlayerManager.ExecuteUpdateMyStatus(normal_number, energy, score);
    }

    public static void handle_update_normal_numbers_response(int num)
    {
        GameManager.normal_number = num;
    }
    public static void handle_update_health_response(int health)
    {
        GameManager.health = health;
    }
    public static void handle_update_velocity(float v)
    {
        var hero = PlayerManager.user2player[GameManager.user_id];
        var heroController = hero.GetComponent<PlayerController>();
        heroController.moveSpeed = v/20;
    }

    public static void handle_spawn_box_response(List<object> X, List<object> Y)
    {
        var mapManager = GameObject.Find("Terrain").GetComponent<MapManager>();
        for(int i = 0; i < X.Count; i++) {
            mapManager.AddBox((float)X[i], (float)Y[i]);
        }
    }
    public static void handle_upgrade_character_response(int money, int lv)
    {
        Hero.ExecuteUpgrade(lv, money);
    }

}

internal struct NewStruct
{
    public object Item1;
    public object Item2;

    public NewStruct(object item1, object item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public override bool Equals(object obj)
    {
        return obj is NewStruct other &&
               System.Collections.Generic.EqualityComparer<object>.Default.Equals(Item1, other.Item1) &&
               System.Collections.Generic.EqualityComparer<object>.Default.Equals(Item2, other.Item2);
    }

    public override int GetHashCode()
    {
        int hashCode = -1030903623;
        hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<object>.Default.GetHashCode(Item1);
        hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<object>.Default.GetHashCode(Item2);
        return hashCode;
    }

    public void Deconstruct(out object item1, out object item2)
    {
        item1 = Item1;
        item2 = Item2;
    }

    public static implicit operator (object, object)(NewStruct value)
    {
        return (value.Item1, value.Item2);
    }

    public static implicit operator NewStruct((object, object) value)
    {
        return new NewStruct(value.Item1, value.Item2);
    }
}