import sqlite3

# lv_s = shooter level lv_b = bomber level lv_t = tank level
conn = sqlite3.connect('../test.db')
c = conn.cursor()

c.execute('''CREATE TABLE IF NOT EXISTS user (
id integer PRIMARY KEY AUTOINCREMENT,
name text NOT NULL,
password text NOT NULL,
money integer NOT NULL,
lv_s integer NOT NULL,
lv_b integer NOT NULL,
lv_t integer NOT NULL
);''')

test_users = [
    ('test1', '163', 100, 1, 1, 1),
    ('test2', '163', 100, 1, 1, 1),
    ('test3', '163', 100, 1, 1, 1)
]
c.executemany('''
INSERT INTO user VALUES (NULL,?,?,?,?,?,?)
''', test_users)

c.execute('''CREATE TABLE IF NOT EXISTS character (
id integer PRIMARY KEY AUTOINCREMENT,
character_type text NOT NULL,
health integer NOT NULL,
normal_damage integer NOT NULL,
combo_damage integer NOT NULL,
velocity integer NOT NULL
);''')

test_characters = [
    ('shooter', 3500, 300, 600, 20),
    ('bomber', 2500, 600, 1200, 15),
    ('tank', 5000, 400, 800, 25)
]
c.executemany('''
INSERT INTO character VALUES (NULL,?,?,?,?,?)
''', test_characters)

conn.commit()
conn.close()
