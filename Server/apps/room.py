from apps.game import Game
from apps.player import PlayerPool, Player


# RoomStatus = Enum('RoomStatus', ('WAIT', 'GAME', 'EMPTY', 'FULL'))

class Room:
    def __init__(self, room_id, player):
        self.room_id = room_id
        self.players = [player]
        self.max_num = 10
        self.status = "WAIT"
        self.master = player
        self.game = None
        # t = threading.Thread(target=self.update_room_content)
        # t.start()

    def JoinRoom(self, player):
        if self.status == 'WAIT':
            self.players.append(player)
            if len(self.players) == self.max_num:
                self.status = 'FULL'
            player.room = self
            return self.room_id
        else:
            return -1

    def LeaveRoom(self, player):
        player.room = None
        if player in self.players:
            self.players.remove(player)
            if len(self.players) > 0:
                if player == self.master:
                    self.master = self.players[0]
            else:
                self.status = "EMPTY"
        return

    def StartGame(self):
        can_start = True
        for player in self.players:
            if player.status != 'READY':
                can_start = False
                return -1
        self.status = "GAME"
        for player in self.players:
            player.status = "GAME"
        self.game = Game(self, self.players)
        self.game.Start()
        return 0

    # def BroadcastRoom(self, msg_type, data):
    #     for each_player in self.players:
    #         each_player.Send(msg_type, data)
    #

    def BroadcastRPC(self, rpc_func):
        for each_player in self.players:
            if each_player.user_id > 0:  # human, not robot
                rpc_func(each_player.gameEntity)


class RoomPool:
    def __init__(self):
        pass

    rooms = {}

    @classmethod
    def CreateRoom(cls, user_id):
        player = Player(user_id)  # get all user info
        PlayerPool.uid2player[user_id] = player
        room_id = 1000 + len(cls.rooms)
        room = Room(room_id, player)
        cls.rooms[room_id] = room
        player.room = room
        return room_id

    @classmethod
    def JoinRoom(cls, user_id, room_id):
        player = Player(user_id)
        PlayerPool.uid2player[user_id] = player
        room = cls.rooms[room_id]
        if not room:
            return -1, -1
        room_id = room.JoinRoom(player)
        if room_id < 0:
            return -1, -1
        else:
            return user_id, room_id

    @classmethod
    def UpdatePlayerboard(cls, room_id):
        room = cls.rooms[room_id]
        players = room.players
        # print len(players)
        return [x.name for x in players], [x.game_status.character for x in players], room.master.name, room_id

    @classmethod
    def UpdatePlayerStatus(cls, room_id):
        room = cls.rooms[room_id]
        players = room.players
        return [x.user_id for x in players if x.status != "DEAD"], \
               [x.game_status.health for x in players if x.status != "DEAD"], \
               [x.game_status.max_health for x in players if x.status != "DEAD"], \
               [x.game_status.buff for x in players if x.status != "DEAD"]

    @classmethod
    def IsMaster(cls, user_id, room_id):
        player = PlayerPool.Get(user_id)
        room = cls.rooms[room_id]
        if room.master == player:
            return 1
        else:
            return 0

    @classmethod
    def UpdateRoomboard(cls):
        waiting_rooms = {}
        gaming_rooms = {}
        empty_rooms = {}
        full_rooms = {}
        if len(cls.rooms) > 0:
            for key in cls.rooms:
                if cls.rooms[key].status == "WAIT":
                    waiting_rooms[str(key)] = len(cls.rooms[key].players)
                elif cls.rooms[key].status == "GAME":
                    gaming_rooms[str(key)] = len(cls.rooms[key].players)
                elif cls.rooms[key].status == 'FULL':
                    full_rooms[str(key)] = 10
                else:
                    empty_rooms[str(key)] = 0
            sorted(waiting_rooms.items(), key=lambda r: r[1])
            sorted(gaming_rooms.items(), key=lambda r: r[1])
        # print "rooms length: " + str(len(cls.rooms))
        return len(cls.rooms) - len(empty_rooms), len(waiting_rooms), waiting_rooms, gaming_rooms

    # @classmethod
    # def ExitRoom(cls, user_id, room_id):
    #     player = PlayerPool.uid2player[user_id]
    #     room = cls.rooms[room_id]
    #     room.LeaveRoom(player)
    #     if room.status == 'EMPTY':
    #         del cls.rooms[room_id]
    #     PlayerPool.RemovePlayer(user_id)
    #     return

    # @classmethod
    # def StartGame(cls, room_id):
    #     room = cls.rooms[room_id]
    #     if room.StartGame() < 0:
    #         return -1
    #     return room
