import random

from apps.map import Map
from player import PlayerPool
from robot import Robot, RobotManager


def genUniqueID(room_id, robot_id):
    return -room_id * 10 - robot_id


class Game:
    def __init__(self, room, players):
        self.room = room
        self.players = players
        self.map = Map()
        self.player_num = len(players)
        self.players += [Robot(genUniqueID(room.room_id, i), self) for i in range(10 - self.player_num)]
        self.uid2player = {x.user_id: x for x in self.players}
        for robot in self.players[self.player_num:]:
            robot.CloneFromPlayer(random.choice(self.players[:self.player_num]))
            PlayerPool.uid2player[robot.user_id] = robot
            RobotManager.AddRobot(robot)

    def Start(self):
        self.SpawnBox(10)
        self.SpawnPlayers()
        for i in range(self.player_num, len(self.players)):
            self.players[i].status = "GAME"

    def SpawnBox(self, k):
        self.box_pos = self.map.GenBoxPos(k)

    def SpawnBoxRPC(self, player):
        if not self.box_pos:
            return
        player.gameEntity.caller.handle_spawn_box_response([pos[0] for pos in self.box_pos],
                                                           [pos[1] for pos in self.box_pos])

    def SpawnPlayers(self):
        born_pos = self.map.GenBornPos(k=len(self.players))
        for i in range(len(self.players)):
            self.players[i].MoveTo(born_pos[i][0], born_pos[i][1])
            print self.players[i].user_id, born_pos[i]
        self.born_resp = [
            [
                self.players[i].user_id,
                self.players[i].name,
                self.players[i].game_status.character,
                self.players[i].game_status.health,
                self.players[i].game_status.normal_damage,
                self.players[i].game_status.combo_damage,
                self.players[i].game_status.velocity,
                self.players[i].game_status.normal_number,
                self.players[i].game_status.energy,
                self.players[i].game_status.buff,
                self.players[i].game_status.score,
                born_pos[i][0],
                born_pos[i][1],
            ]
            for i in range(len(born_pos))
        ]

    def SpawnPlayersRPC(self, player):
        for each in self.born_resp:
            player.gameEntity.caller.handle_born_at_response(*each)

    def Move(self, user_id, x, y):
        self.uid2player[user_id].MoveTo(x, y)
        self.room.BroadcastRPC(lambda c: c.caller.handle_move_to_response(
            user_id,
            x, y
        ))

    def SafeMove(self, user_id, x, y, origin_x, origin_y):
        x, y = self.CheckMove(x, y, origin_x, origin_y)
        self.Move(user_id, x, y)

    def CheckMove(self, x, y, origin_x, origin_y):
        if self.map.InWall(x, y) or self.map.OutSide(x, y):
            return origin_x, origin_y
        return x, y

    def MoveIncremental(self, x, y, origin_x, origin_y, step=5):
        delta_x = (x - origin_x) / step
        delta_y = (y - origin_y) / step
        for i in range(step):
            tmp_x, tmp_y = self.CheckMove(origin_x + delta_x, origin_y + delta_y, origin_x, origin_y)
            if tmp_x == origin_x and tmp_y == origin_y:
                return tmp_x, tmp_y
            origin_x += delta_x
            origin_y += delta_y
        return x, y
