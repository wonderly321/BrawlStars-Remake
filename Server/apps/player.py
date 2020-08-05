# from message import Message
import math
import time

from user_model import fetch_character_info_by_type, fetch_user_by_id


class GameStatus:
    def __init__(self, user_id):
        # self.Reset()
        # self.game_status = "WAIT"
        self.character = "shooter"
        user = fetch_user_by_id(user_id)
        # get level
        user_info = dict(zip(['id', 'name', 'password', 'money', 'lv_s', 'lv_b', 'lv_t'], user))
        characters = {"shooter": "lv_s", "tank": "lv_b", "bomber": "lv_t"}
        self.lv = user_info[characters[self.character]]
        # get character base properties
        character = fetch_character_info_by_type(self.character)
        c_info = dict(zip(['id', 'character_type', 'health', 'normal_damage', 'combo_damage', 'velocity'], character))
        # properties
        self.health = c_info['health'] + 100 * self.lv
        self.normal_damage = c_info['normal_damage'] + 50 * self.lv
        self.combo_damage = c_info['combo_damage'] + 50 * self.lv
        self.velocity = c_info['velocity'] + self.lv
        self.normal_number = 3
        self.energy = 0  # full : 3
        # info
        self.position = (0, 0)
        self.score = 0
        self.dead = 0
        self.max_health = self.health
        self.buff = 0

    def AddBuff(self, _t=1):
        self.buff = self.buff + _t
        self.buff = max(0, self.buff)

    def Refresh(self, character):
        self.character = character
        character = fetch_character_info_by_type(self.character)
        c_info = dict(zip(['id', 'character_type', 'health', 'normal_damage', 'combo_damage', 'velocity'], character))
        self.health = c_info['health'] + 100 * self.lv
        self.normal_damage = c_info['normal_damage'] + 50 * self.lv
        self.combo_damage = c_info['combo_damage'] + 50 * self.lv
        self.velocity = c_info['velocity'] + self.lv
        self.max_health = self.health

    def SetPos(self, x, z):
        self.position = (x, z)
        return x, z

    def Damage(self, hurt):
        self.health -= hurt
        self.health = max(0, self.health)
        return self.health

    def UpdateNormalAttacks(self):
        self.normal_number += 1
        self.normal_number = min(3, self.normal_number)
        return self.normal_number

    def UpdateHealth(self):
        self.health += 200
        self.health = min(self.max_health, self.health)


# PlayerStatus = Enum('PlayerStatus', ('WAIT', 'READY', 'GAME'))

class Player:
    def __init__(self, user_id):
        self.status = 'WAIT'
        self.user_id = user_id
        user = fetch_user_by_id(user_id)
        user_info = dict(zip(['id', 'name', 'password', 'money', 'lv_s', 'lv_b', 'lv_t'], user))
        self.name = user_info['name']
        self.room = None
        self.game_status = GameStatus(user_id)
        self.gameEntity = None

    # def Reconnect(self, info):
    #     print(info)
    #     self.reconnect_info = info

    # def Renew(self, broker):
    #     self.broker = broker
    #     if self.reconnect_info:
    #         self.Send("Reconnect", self.reconnect_info)
    #         self.reconnect_info = None

    # def Send(self, msg_type, msg_dict):
    #     if self.broker.active:
    #         # msg = Message.response(msg_type, msg_dict)
    #         # logging.info('<%s> Broadcast %s'%(self.name, binascii.b2a_hex(msg).decode('utf-8')))
    #         # self.broker.sock.send(msg)
    #     else:
    #         self.LostConnect()

    # def LostConnect(self):
    #     if self.room and self.game_status.gaming is False:
    #         self.room.LeaveRoom(self)

    def ChooseCharacter(self, char):
        # self.game_status.character = char
        self.game_status.Refresh(char)
        self.status = 'READY'

    def MoveTo(self, x, z):
        return self.game_status.SetPos(x, z)

    def Repel(self, dir_x, dir_z, s):
        pos = self.game_status.position
        arrow_s = math.sqrt(math.pow(dir_x, 2) + math.pow(dir_z, 2))  # arrow's translation
        move_x = (dir_x * s) / arrow_s
        move_z = (dir_z * s) / arrow_s
        self.MoveTo(pos[0] + move_x, pos[1] + move_z)

    def Slowdown(self, count):
        self.gameEntity.caller.handle_update_velocity_response(self.game_status.velocity * (1 - count))
        time.sleep(1)
        self.gameEntity.caller.handle_update_velocity_response(self.game_status.velocity)


class PlayerPool:
    def __init__(self):
        pass

    uid2player = {}

    # @classmethod
    # def NewPlayer(cls, user_id):
    #     if user_id not in cls.uid2player:
    #         cls.uid2player[user_id] = Player(user_id)
    #     else:
    #         # cls.uid2player[user_id].Renew(broker)
    #         print 'user already in uis2player'
    #
    # @classmethod
    # def Get(cls, user_id):
    #     return cls.uid2player.get(user_id, None)

    # @classmethod
    # def ChooseCharacter(cls, user_id, character):
    #     player = cls.uid2player[user_id]
    #     player.ChooseCharacter(character)
    #     return player.game_status.character

    @classmethod
    def RemovePlayer(cls, user_id):
        del cls.uid2player[user_id]
