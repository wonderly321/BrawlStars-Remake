import sqlite3


class DB:
    @classmethod
    def execute(cls, sql):
        conn = sqlite3.connect('test.db')
        cursor = conn.cursor()
        ret = cursor.execute(sql).fetchall()
        conn.commit()
        conn.close()
        return ret

    @classmethod
    def executemany(cls, sql, l):
        conn = sqlite3.connect('test.db')
        cursor = conn.cursor()
        ret = cursor.executemany(sql, l).fetchall()
        conn.commit()
        conn.close()
        return ret
