import os
import random

map_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'map.txt')


def LoadMapFromTxt(filename=map_file):
    with open(filename, 'r') as f:
        raw_data = f.readlines()
        raw_data = [x.strip() for x in raw_data]
        raw_data = [x.split(' ') for x in raw_data]
    return list(map(lambda l: [int(each) for each in l], raw_data))


def GetTerrain(_map):
    n = len(_map)
    grass, ground, wall = set(), set(), set()
    for i in range(n):
        for j in range(n):
            if _map[i][j] == 0:
                ground.add((i, j))
            elif _map[i][j] == 1:
                grass.add((i, j))
            elif _map[i][j] == 2:
                wall.add((i, j))
    return grass, ground, wall


def Pos_LogicToWorld(p, q):
    return -15.5 + p, 15.5 - q


def Pos_WorldToLogic(x, y):
    return int(round(x + 15.5)), int(round(15.5 - y))


class Map:
    def __init__(self):
        self.map = LoadMapFromTxt()
        self.grass, self.ground, self.wall = GetTerrain(self.map)
        self.box = set()

    def GenBoxPos(self, k=20):
        self.box = set(random.sample(self.ground, k))
        for each in self.box:
            self.map[each[0]][each[1]] = 5  # 5: box
        return [Pos_LogicToWorld(*x) for x in self.box]

    def GenBornPos(self, k=10):
        ret = random.sample(self.ground - self.box, k)
        return [Pos_LogicToWorld(*x) for x in ret]

    def InWall(self, x, y, d=0.39):
        for each_wall in set.union(self.wall, self.box):
            wall_x, wall_y = Pos_LogicToWorld(*each_wall)
            if wall_x - 0.5 - d <= x and wall_x + 0.5 + d >= x and wall_y - 0.5 - d <= y and wall_y + 0.5 + d >= y:
                return True
        return False

    def OutSide(self, x, y, d=0.39):
        if abs(x) <= 15.5 - d and abs(y) <= 15.5 - d:
            return False
        return True

    def DestroyWall(self, x, z):
        x, z = Pos_WorldToLogic(x, z)
        self.map[x][z] = 0
        if (x, z) in self.wall:
            self.wall.remove((x, z))
        self.ground.add((x, z))


if __name__ == '__main__':
    basicMap = Map()
