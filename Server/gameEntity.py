from apps.player import PlayerPool
from apps.room import RoomPool
from apps.user import User
from netStream import RpcProxy
from user_model import update_user_money, fetch_user_by_id, update_user_lv_s, update_user_lv_b, update_user_lv_t


def EXPOSED(func):
    func.__exposed__ = True
    return func


class GameEntity(object):
    EXPOSED_FUNC = {}

    def __init__(self, netstream):
        self.netstream = netstream
        self.caller = RpcProxy(self, netstream)
        self.stat = 0

    def destroy(self):
        self.caller = None
        self.netstream = None

    @EXPOSED
    def hello_world_from_client(self, stat, msg):
        print 'server recv msg from client:', stat, msg
        self.stat = stat + 1
        self.caller.recv_msg_from_server(self.stat, msg)

    @EXPOSED
    def login(self, name, pwd):
        print 'server recv login: ' + name + ' ' + pwd
        self.caller.handle_login_response(*User.Login(name, pwd))

    @EXPOSED
    def register(self, name, pwd):
        self.caller.handle_register_response(User.Register(name, pwd))

    @EXPOSED
    def create_room(self, user_id):
        room_id = RoomPool.CreateRoom(user_id)
        if room_id > 0:
            PlayerPool.uid2player[user_id].gameEntity = self
        print 'user [' + str(user_id) + '] create and join room [' + str(room_id) + ']'
        self.caller.handle_join_room_response(user_id, room_id)

    @EXPOSED
    def join_room(self, user_id, room_id):
        user_id, room_id = RoomPool.JoinRoom(user_id, room_id)
        if user_id > 0:
            PlayerPool.uid2player[user_id].gameEntity = self
            print 'user [' + str(user_id) + ']  join in room [' + str(room_id) + ']'
        else:
            print 'user [' + str(user_id) + ']  CANNOT join in room [' + str(room_id) + ']'
        self.caller.handle_join_room_response(user_id, room_id)

    @EXPOSED
    def update_room_board(self):
        # print "Recv update roomboard from client"
        self.caller.handle_update_roomboard_response(*RoomPool.UpdateRoomboard())

    @EXPOSED
    def exit_room(self, user_id, room_id):
        room = RoomPool.rooms[room_id]
        if not room:
            print "[" + str(room_id) + "] not exist"
            self.caller.handle_exit_room_response(-1)
        player = PlayerPool.uid2player[user_id]
        if not player:
            print "[" + str(user_id) + "] not exist"
            self.caller.handle_exit_room_response(-1)
        room.LeaveRoom(player)
        if room.status == 'EMPTY':
            del RoomPool.rooms[room_id]
        PlayerPool.RemovePlayer(user_id)
        print "[" + str(user_id) + "] exit room [" + str(room_id) + "]"
        self.caller.handle_exit_room_response(room_id)

    @EXPOSED
    def update_player_board(self, room_id):
        # print "Recv update playerboard from client"
        self.caller.handle_update_playerboard_response(*RoomPool.UpdatePlayerboard(room_id))

    @EXPOSED
    def choose_character(self, user_id, character):
        player = PlayerPool.uid2player[user_id]
        player.ChooseCharacter(character)
        print "user [" + str(user_id) + "] choose character [" + character + "]"
        self.caller.handle_choose_character_response(player.game_status.character)

    @EXPOSED
    def start_game(self, room_id):
        room = RoomPool.rooms[room_id]
        if room.StartGame() == 0:
            print "room [" + str(room_id) + "] has started game"
            room.BroadcastRPC(lambda x: x.caller.handle_start_game_response(0))
        else:
            print "room [" + str(room_id) + "] can't start"
            self.caller.handle_start_game_response(-1)

    @EXPOSED
    def spawn_box(self, user_id):
        player = PlayerPool.uid2player[user_id]
        game = player.room.game
        game.SpawnBoxRPC(player)

    @EXPOSED
    def spawn_players(self, user_id):
        player = PlayerPool.uid2player[user_id]
        game = player.room.game
        game.SpawnPlayersRPC(player)

    @EXPOSED
    def move_to(self, user_id, x, z, origin_x, origin_z):
        player = PlayerPool.uid2player[user_id]
        new_x, new_z = player.room.game.CheckMove(x, z, origin_x, origin_z)
        player.room.game.Move(user_id, new_x, new_z)

    @EXPOSED
    def normal_attack(self, user_id, y):
        player = PlayerPool.uid2player[user_id]
        if not player:
            return
        if player.game_status.normal_number > 0:
            # print player.name + ' normal number remain : ' + str(player.game_status.normal_number)
            player.game_status.normal_number -= 1
            player.room.BroadcastRPC(
                lambda c: c.caller.handle_normal_attack_response(user_id, y))
        else:
            print player.name + '  no normal attacks'

    @EXPOSED
    def normal_damage(self, user_id, hurt_id, damage):
        player = PlayerPool.uid2player[user_id]
        # player.game_status.normal_number -= 1
        player.game_status.energy += 1
        player.game_status.energy = min(5, player.game_status.energy)

        hurt = PlayerPool.uid2player[hurt_id]
        if hurt.game_status.Damage(damage) == 0:
            hurt.game_status.dead = 1
            hurt.status = "DEAD"
            player.game_status.score += 1
            player.room.BroadcastRPC(lambda c: c.caller.handle_dead_response(hurt_id))

    @EXPOSED
    def combo_attack(self, user_id, x, y, z):
        player = PlayerPool.uid2player[user_id]
        if player.game_status.energy == 5:
            print player.name + ' combo'
            player.game_status.energy = 0
            player.room.BroadcastRPC(
                lambda c: c.caller.handle_combo_attack_response(user_id, x, y, z));
        else:
            print player.name + '  no combo attack'

    @EXPOSED
    def combo_damage(self, user_id, hurt_id, damage, x, z):
        player = PlayerPool.uid2player[user_id]
        player.game_status.energy = 0
        hurt = PlayerPool.uid2player[hurt_id]
        x, z = hurt.room.game.MoveIncremental(x, z, hurt.game_status.position[0], hurt.game_status.position[1])
        hurt.MoveTo(x, z)
        hurt.room.BroadcastRPC(lambda c: c.caller.handle_repel_response(hurt_id, x, z))
        if hurt.game_status.Damage(damage) == 0:
            hurt.game_status.dead = 1
            hurt.status = "DEAD"
            player.game_status.score += 1
            player.room.BroadcastRPC(lambda c: c.caller.handle_dead_response(hurt_id))
        # elif player.game_status.character == "bomber":
        #     hurt.Slowdown(hurt_id, 0.2)  # slowdown 20% last for 1s
        # else:
        #     hurt.gameEntity.caller.handle_dizzy_response(hurt_id, 1)  # dizzy last for 1s

    @EXPOSED
    def update_player_status(self, room_id):
        # print "Recv update playerboard from client"
        self.caller.handle_update_player_status_response(*RoomPool.UpdatePlayerStatus(room_id))

    @EXPOSED
    def update_my_status(self, user_id):
        player = PlayerPool.uid2player[user_id]
        self.caller.handle_update_my_status_response(player.game_status.normal_number, player.game_status.energy,
                                                     player.game_status.score)

    @EXPOSED
    def update_normal_numbers(self, user_id):
        player = PlayerPool.uid2player[user_id]
        player.game_status.UpdateNormalAttacks()
        self.caller.handle_update_normal_numbers_response(player.game_status.normal_number)

    @EXPOSED
    def update_health(self, user_id):
        player = PlayerPool.uid2player[user_id]
        player.game_status.UpdateHealth()
        self.caller.handle_update_health_response(player.game_status.health)

    @EXPOSED
    def destroy_wall(self, uid, x, z):
        player = PlayerPool.uid2player[uid]
        player.room.game.map.DestroyWall(x, z)

    @EXPOSED
    def update_money(self, user_id, money):
        update_user_money(user_id, money)

    @EXPOSED
    def upgrade_character(self, user_id, character):
        user = fetch_user_by_id(user_id)
        user = dict(zip(['id', 'name', 'password', 'money', 'lv_s', 'lv_b', 'lv_t'], user))
        update_user_money(user_id, user['money'] - 50)
        char2lv = {"shooter": "lv_s", "bomber": "lv_b", "tank": "lv_t"}
        lv = char2lv[character]
        if lv == "lv_s":
            update_user_lv_s(user_id, user["lv_s"] + 1)
            self.caller.handle_upgrade_character_response(user['money'] - 50, user["lv_s"] + 1)
        elif lv == "lv_b":
            update_user_lv_b(user_id, user["lv_b"] + 1)
            self.caller.handle_upgrade_character_response(user['money'] - 50, user["lv_b"] + 1)
        else:
            update_user_lv_t(user_id, user["lv_t"] + 1)
            self.caller.handle_upgrade_character_response(user['money'] - 50, user["lv_t"] + 1)

    @EXPOSED
    def exit(self):
        print 'client exit, set stat to -1:'
        self.stat = -1
