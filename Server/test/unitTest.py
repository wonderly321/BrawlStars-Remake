# -*- coding: GBK -*-

import sys
import time
import unittest

sys.path.append('..')
sys.path.append('../common')
sys.path.append('../network')
sys.path.append('../common_server')
sys.path.append('../apps/users')

from apps.user import Users
from apps.room import RoomPool
from apps.player import PlayerPool
from events import MsgCSLogin, MsgCSMoveto
from dispatcher import Service, Dispatcher
from netStream import NetStream, RpcProxy
from simpleHost import SimpleHost
from timer import TimerManager


class TestService(Service):
    def __init__(self, sid=0):
        super(TestService, self).__init__(sid)
        commands = {
            10: self.f,
            20: self.f,
        }
        self.registers(commands)

    def f(self, msg, owner):
        return owner


class MsgService(object):
    pass


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

    # CLIENT CODE
    # @EXPOSED
    # def recv_msg_from_server(self, stat, msg):
    # 	print 'client recv msg from server:', stat, msg
    # 	self.stat = stat
    #
    # 	self.caller.exit()
    ###

    # SERVER CODE
    @EXPOSED
    def hello_world_from_client(self, stat, msg):
        print 'server recv msg from client:', stat, msg
        self.stat = stat + 1
        self.caller.recv_msg_from_server(self.stat, msg)

    @EXPOSED
    def login(self, name, pwd):
        print 'server recv login: ' + name + ' ' + pwd
        self.caller.handle_login_response(*Users.Login(name, pwd))

    @EXPOSED
    def register(self, name, pwd):
        self.caller.handle_register_response(Users.Register(name, pwd))

    @EXPOSED
    def create_room(self, user_id):
        print 'create room by user: ' + str(user_id)
        ret = RoomPool.CreateRoom(user_id)
        if ret > 0:
            PlayerPool.uid2player[user_id].gameEntity = self
        self.caller.handle_join_room_response(ret)

    @EXPOSED
    def join_room(self, user_id, room_id):
        print  '[' + str(user_id) + ']  join in  [' + str(room_id) + ']'
        user_id, room_id = RoomPool.JoinRoom(user_id, room_id)
        if user_id > 0:
            Room = RoomPool.rooms[room_id]
            Room.BroadcastRPC(lambda x: x.caller.handle_join_room_response(user_id, room_id))
        else:
            self.caller.handle_join_room_response(user_id, room_id)

    @EXPOSED
    def update_room_board(self):
        # print "Recv update roomboard from client"
        self.caller.handle_update_roomboard_response(*RoomPool.UpdateRoomboard())

    @EXPOSED
    def exit_room(self, user_id, room_id):
        print "[" + str(user_id) + "] exit room [" + str(room_id) + "]"
        self.caller.handle_exit_room_response(RoomPool.ExitRoom(user_id, room_id))

    @EXPOSED
    def update_player_board(self, room_id):
        # print "Recv update playerboard from client"
        self.caller.handle_update_playerboard_response(*RoomPool.UpdatePlayerboard(room_id))

    @EXPOSED
    def choose_character(self, user_id, charater):
        self.caller.handle_choose_character_response(PlayerPool.ChooseCharacter(user_id, charater))

    @EXPOSED
    def start_game(self, room_id):
        room = RoomPool.rooms[room_id]
        if room.StartGame() == 0:
            room.BroadcastRPC(lambda x: x.caller.handle_start_game_response(0))
        else:
            self.caller.handle_start_game_response(-1)

    @EXPOSED
    def move_to(self, user_id, vector):
        player = PlayerPool.uid2player[user_id]
        room = player.room
        pos = player.MoveTo(vector)
        room.BroadcastRPC(lambda x: x.caller.handle_move_to_response(user_id, pos))

    @EXPOSED
    def exit(self):
        print 'client exit, set stat to -1:'
        self.stat = -1


###


class ServerTest(unittest.TestCase):
    def setUp(self):
        self._head1 = MsgCSLogin('test', 0)
        self._head2 = MsgCSMoveto(3, 5)

        self._dispatcher = Dispatcher()
        self._dispatcher.register(100, TestService())

        self.count = 0

    def tearDown(self):
        self._head1 = None
        self._head2 = None

        self._dispatcher = None

        self.count = 0

    def addCount(self):
        self.count += 1

    def test_Parser(self):
        # test header

        data = self._head1.marshal()
        head = MsgCSLogin().unmarshal(data)
        self.assertEqual(self._head1.name, head.name)
        self.assertEqual(self._head1.icon, head.icon)

        data = self._head2.marshal()
        head = MsgCSMoveto().unmarshal(data)
        self.assertEqual(self._head2.x, head.x)
        self.assertEqual(self._head2.y, head.y)

        # test dispatcher
        msg = MsgService()
        msg.sid = 100
        msg.cid = 10
        self.assertEqual(self._dispatcher.dispatch(msg, 'client1'), 'client1')
        msg.cid = 20
        self.assertEqual(self._dispatcher.dispatch(msg, 'client2'), 'client2')

        # test network
        host = SimpleHost()
        host.startup(2000)
        sock = NetStream()
        last = time.time()
        sock.connect('127.0.0.1', 2000)

        stat = 0
        last = time.time()
        sock.nodelay(1)

        client_entity = None
        server_entities = {}

        while 1:
            time.sleep(0.1)

            ### CLIENT SECTION
            # sock.process()
            # if stat == 0:
            # 	if sock.status() == conf.NET_STATE_ESTABLISHED:
            # 		stat = 1
            # 		client_entity = GameEntity(sock)
            # 		client_entity.caller.hello_world_from_client(stat, 'Hello, world !!')
            # 		last = time.time()
            # else:
            # 	recv_data = sock.recv()
            # 	if len(recv_data) > 0:
            # 		client_entity.caller.parse_rpc(recv_data)
            ####

            ### SERVER SECTION
            host.process()
            event, wparam, data = host.read()
            if event < 0:
                continue

            # if event == conf.NET_CONNECTION_NEW:
            #     code, client_netstream = host.getClient(wparam)
            #     self.assertGreaterEqual(code, 0)
            #     server_entity = GameEntity(client_netstream)
            #     server_entities[client_netstream] = server_entity
            # # print server_entity.stat
            #
            #
            # elif event == conf.NET_CONNECTION_DATA:
            #     code, client_netstream = host.getClient(wparam)
            #     server_entity = server_entities[client_netstream]
            #     server_entity.caller.parse_rpc(data)
            #     # print server_entity.stat
            #
            #     if server_entity.stat == -1:
            #         server_entity.destroy()
            #         host.closeClient(wparam)
            #         host.shutdown()
            #         break
        ###

        # test timer
        TimerManager.addRepeatTimer(0.15, self.addCount)
        last = time.time()
        while 1:
            time.sleep(0.01)
            TimerManager.scheduler()

            if time.time() - last > 1.0:
                break

        self.assertEqual(self.count, 6)

        return


if __name__ == '__main__':
    unittest.main()
