import copy
import math
import random

from map import Pos_WorldToLogic, Pos_LogicToWorld


def math_dist((x0, y0), (x1, y1)):
    return math.sqrt((x0 - x1) ** 2 + (y0 - y1) ** 2)


class RobotState:
    IDLE = 0
    TOMOVE = 1
    MOVE = 2
    CAN_SHOOT = 3
    HANGOUT = 4


class Robot():
    def __init__(self, user_id, game):
        self.user_id = user_id
        self.status = "WAIT"
        self.name = "robot_%d" % (user_id)
        self.game = game
        self.state = RobotState.IDLE
        self.deltaTime = 0.2
        self.game_status = None
        self.room = None
        self.attack_radius = 2
        self.visited = set()
        self.next_index = 0

    def CloneFromPlayer(self, player):
        self.game_status = copy.deepcopy(player.game_status)
        self.room = player.room

    def check(self, x, y):
        if (x, y) in self.visited:
            return False
        if x < 0 or x > 31 or y < 0 or y > 31:
            return False
        if self.game.map.map[x][y] > 2:
            return False
        return True

    def Tick(self):
        if self.state == RobotState.IDLE:
            self.target = self.find_target()
            self.state = RobotState.TOMOVE
            if self.target.status == "DEAD":
                self.state = RobotState.IDLE
            for player in self.game.players:
                if self.user_id == player.user_id: continue
                if self.attack_radius >= math_dist(player.game_status.position, self.game_status.position):
                    self.target = player
                    self.state = RobotState.CAN_SHOOT
                    return
        elif self.state == RobotState.TOMOVE:
            self.plan_path = self.find_way(
                Pos_WorldToLogic(*self.game_status.position),
                Pos_WorldToLogic(*self.target.game_status.position)
            )
            # if self.plan_path:
            #     print self.plan_path
            #     self.plan_step = 0
            #     self.state = RobotState.MOVE
            # else:
            #     self.state = RobotState.HANGOUT
            self.state = RobotState.HANGOUT
        elif self.state == RobotState.MOVE:
            move_limit = self.game_status.velocity / 10 * self.deltaTime
            if Pos_WorldToLogic(self.game_status.position) == self.plan_path[self.plan_step]:
                self.plan_step += 1
            old_x, old_y = self.game_status.position
            x, y = Pos_LogicToWorld(*self.plan_path[self.plan_step])
            middle_target_dist = math_dist(
                (old_x, old_y),
                (x, y)
            )
            scale = min(1.0, move_limit / middle_target_dist)
            new_x = old_x + (x - old_x) * scale
            new_y = old_y + (y - old_y) * scale
            self.game.SafeMove(self.user_id, new_x, new_y, old_x, old_y)

            if self.target.status == "DEAD":
                self.state = RobotState.IDLE
            for player in self.game.players:
                if self.user_id == player.user_id: continue
                if self.attack_radius >= math_dist(player.game_status.position, self.game_status.position):
                    self.target = player
                    self.state = RobotState.CAN_SHOOT
                    break
        elif self.state == RobotState.HANGOUT:
            x, y = Pos_WorldToLogic(*self.game_status.position)
            next = []
            for d in [-1, 1]:
                if self.check(x + d, y):
                    next.append((x + d, y))
                if self.check(x, y + d):
                    next.append((x, y + d))
            if not next:
                self.visited.clear()
            else:

                if random.randint(0, 4) == 0:
                    self.next_index = random.randint(0, len(next) - 1)
                next = next[min(self.next_index, len(next) - 1)]
                move_limit = self.game_status.velocity / 20 * self.deltaTime
                old_x, old_y = self.game_status.position
                x, y = Pos_LogicToWorld(*next)
                middle_target_dist = math_dist(
                    (old_x, old_y),
                    (x, y)
                )
                scale = min(1.0, move_limit / middle_target_dist)
                new_x = old_x + (x - old_x) * scale
                new_y = old_y + (y - old_y) * scale
                if new_x == old_x and new_y == old_y:
                    return
                self.game.SafeMove(self.user_id, new_x, new_y, old_x, old_y)

            if self.target.status == "DEAD":
                self.state = RobotState.IDLE
            for player in self.game.players:
                if self.user_id == player.user_id: continue
                if self.attack_radius >= math_dist(player.game_status.position, self.game_status.position):
                    self.target = player
                    self.state = RobotState.CAN_SHOOT
                    break

        elif self.state == RobotState.CAN_SHOOT:
            if self.target.status == "DEAD":
                self.state = RobotState.IDLE
            else:
                if self.game_status.normal_number > 0:
                    self.game_status.normal_number -= 1
                    y = math.atan2(
                        self.target.game_status.position[1] - self.game_status.position[1],
                        self.target.game_status.position[0] - self.game_status.position[0]
                    )
                    y = y / math.pi * 180
                    self.room.BroadcastRPC(
                        lambda c: c.caller.handle_normal_attack_response(self.user_id, y))
                    self.game_status.energy += 1
                    self.game_status.energy = min(5, self.game_status.energy)
                    if self.target.game_status.Damage(self.game_status.normal_damage) == 0:
                        self.target.game_status.dead = 1
                        self.target.status = "DEAD"
                        self.game_status.score += 1
                        self.room.BroadcastRPC(lambda c: c.caller.handle_dead_response(self.target.user_id))
            for player in self.game.players:
                if self.user_id == player.user_id: continue
                if self.attack_radius >= math_dist(player.game_status.position, self.game_status.position):
                    self.target = player
                    self.state = RobotState.CAN_SHOOT
                    return
            self.state = RobotState.HANGOUT

    def find_target(self):
        # Find Nearest Target
        minDist = float('Inf')
        target = None
        for player in self.game.players:
            if player.user_id == self.user_id:
                continue
            d = math_dist(self.game_status.position, player.game_status.position);
            if d < minDist:
                minDist = d
                target = player
        return target

    def find_way(self, (x0, y0), (x1, y1)):
        map = self.game.map.map

        def ifHasGreedy(x, y):
            delta_x = 1 if x1 - x > 0 else (-1 if x1 - x < 0 else 0)
            delta_y = 1 if y1 - y > 0 else (-1 if y1 - y < 0 else 0)
            if delta_x == 0 or delta_y == 0:
                if map[x + delta_x][y + delta_y] < 2:
                    return True, delta_x, delta_y
            else:
                if map[x + delta_x][y] < 2 and map[x][y + delta_y] < 2:
                    return True, delta_x, delta_y
            return False, delta_x, delta_y

        q = [(x0, y0)]
        visited = set()
        visited.add(q[0])
        path = {}

        def check(x, y):
            if (x, y) in visited:
                return False
            if x < 0 or x > 31 or y < 0 or y > 31:
                return False
            if map[x][y] > 2:
                return False
            return True

        while len(q) > 0:
            now_x, now_y = q.pop(0)
            hasGreedy, dx, dy = ifHasGreedy(now_x, now_y)
            if hasGreedy:
                now_x += dx
                now_y += dy
                if check(now_x, now_y):
                    path[(now_x, now_y)] = (now_x - dx, now_y - dy)
                    q.append((now_x, now_y))
                    visited.add((now_x, now_y))
            else:
                ## split branch
                if dx == 0:
                    if check(now_x, now_y + 1):
                        path[(now_x, now_y + 1)] = (now_x, now_y)
                        q.append((now_x, now_y + 1))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
                    if check(now_x, now_y - 1):
                        path[(now_x, now_y - 1)] = (now_x, now_y)
                        q.append((now_x, now_y - 1))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
                elif dy == 0:
                    if check(now_x + 1, now_y):
                        path[(now_x + 1, now_y)] = (now_x, now_y)
                        q.append((now_x + 1, now_y))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
                    if check(now_x - 1, now_y):
                        path[(now_x - 1, now_y)] = (now_x, now_y)
                        q.append((now_x - 1, now_y))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
                else:
                    if check(now_x + dx, now_y):
                        path[(now_x + dx, now_y)] = (now_x, now_y)
                        q.append((now_x + dx, now_y))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
                    if check(now_x, now_y + dy):
                        path[(now_x, now_y + dy)] = (now_x, now_y)
                        q.append((now_x, now_y + dy))
                        visited.add(q[-1])
                        if q[-1][0] == x1 and q[-1][1] == y1:
                            break
        if not q:
            return None
        if q[-1][0] == x1 and q[-1][1] == y1:
            tx, ty = x1, y1
            ret = [(tx, ty)]
            while True:
                tx, ty = path[(tx, ty)]
                if tx == x0 and ty == y1:
                    break
                ret.append((tx, ty))
            ret = list(reversed(ret))
            return ret
        return None

    # def Repel(self, dir_x, dir_z, s):
    #     pos = self.game_status.position
    #     arrow_s = math.sqrt(math.pow(dir_x, 2) + math.pow(dir_z, 2))  # arrow's translation
    #     move_x = (dir_x * s) / arrow_s
    #     move_z = (dir_z * s) / arrow_s
    #     self.MoveTo(pos[0] + move_x, pos[1] + move_z)

    def MoveTo(self, x, z):
        return self.game_status.SetPos(x, z)


class RobotManager:
    robots = []
    times = 0

    @classmethod
    def AddRobot(cls, robot):
        cls.robots.append(robot)

    @classmethod
    def tick(cls):
        cls.times += 1
        if cls.times < 10: return
        robots = []
        for robot in cls.robots:
            if robot.status != "DEAD":
                robots.append(robot)
        cls.robots = robots
        for robot in cls.robots:
            if robot.status == "GAME":
                robot.Tick()
