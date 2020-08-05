from service.db import DB


def fetch_user_by_name(name):
    ret = DB.execute('SELECT * FROM user WHERE name="%s"' % name)
    return ret[0] if ret else None


def fetch_user_by_id(id):
    ret = DB.execute('SELECT * FROM user WHERE id="%i"' % id)
    return ret[0] if ret else None


def insert_new_user(name, pwd, money, lv_s, lv_b, lv_t):
    ret = DB.execute(
        '''INSERT INTO user VALUES (NULL,"%s","%s","%i","%i", "%i", "%i");''' % (name, pwd, money, lv_s, lv_b, lv_t))
    ret = fetch_user_by_name(name)
    return ret


def fetch_character_info_by_type(character_type):
    ret = DB.execute('SELECT * FROM character WHERE character_type ="%s"' % character_type)
    return ret[0] if ret else None


def update_user_money(uid, money):
    DB.execute('UPDATE user SET money = %i WHERE id = %i' % (money, uid))


def update_user_lv_s(uid, lv_s):
    DB.execute('UPDATE user SET lv_s = %i WHERE id = %i' % (lv_s, uid))


def update_user_lv_b(uid, lv_b):
    DB.execute('UPDATE user SET lv_b = %i WHERE id = %i' % (lv_b, uid))


def update_user_lv_t(uid, lv_t):
    DB.execute('UPDATE user SET lv_t = %i WHERE id = %i' % (lv_t, uid))
