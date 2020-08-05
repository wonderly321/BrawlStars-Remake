from user_model import *


# from apps.player import PlayerPool

class User:
    def __init__(self):
        pass

    @classmethod
    def Login(cls, name, pwd):  # return all user nfo except pwd
        user = fetch_user_by_name(name)
        if not user:
            return -2, "", -1, -1, -1, -1
        user = dict(zip(['id', 'name', 'password', 'money', 'lv_s', 'lv_b', 'lv_t'], user))
        if user['password'] != pwd:
            return -1, "", -1, -1, -1, -1
        return user['id'], user['name'], user['money'], user['lv_s'], user['lv_b'], user['lv_t']

    @classmethod
    def Register(cls, name, pwd):  # return user id
        code = 1
        user = fetch_user_by_name(name)
        if user:
            code = -3  # user aready exists
            return code
        user = insert_new_user(name, pwd, 100, 1, 1, 1)
        user = dict(zip(['id', 'name', 'password', 'money', 'lv_s', 'lv_b', 'lv_t'], user))
        return user["id"]

    # @classmethod
    # def check_online(cls, user_id):
    #     player = PlayerPool.Get(user_id)
    #     if player and player.broker.active:
    #         return True
    #     return False
