# -*- coding: GBK -*-
import sys

sys.path.append('common')
sys.path.append('network')
sys.path.append('common_server')
sys.path.append('apps')
sys.path.append('service')

import time
import uuid

import conf
from gameEntity import GameEntity

from network.simpleHost import SimpleHost
from dispatcher import Dispatcher

from apps.robot import RobotManager


class SimpleServer(object):

    def __init__(self):
        super(SimpleServer, self).__init__()

        self.entities = {}
        self.host = SimpleHost()
        self.dispatcher = Dispatcher()

        return

    def generateEntityID(self, wparam):
        # raise NotImplementedError
        return uuid.uuid3(uuid.NAMESPACE_OID, str(wparam))

    def registerEntity(self, entity, wparam):
        eid = self.generateEntityID(wparam)
        entity.id = eid
        self.entities[eid] = entity

        return

    def tick(self):
        self.host.process()
        event, wparam, data = self.host.read()
        # print wparam
        if event >= 0:
            if event == conf.NET_CONNECTION_NEW:
                code, client_netstream = self.host.getClient(wparam)  # recept() through wparam (= hid)
                server_entity = GameEntity(client_netstream)
                self.registerEntity(server_entity, wparam);
            # self.entities[client_netstream] = server_entity
            # print server_entity.stat

            elif event == conf.NET_CONNECTION_DATA:
                server_entity = self.entities[self.generateEntityID(wparam)]
                server_entity.caller.parse_rpc(data)
                # print server_entity.stat

                if server_entity.stat == -1:
                    server_entity.destroy()
                    self.host.closeClient(wparam)
                    self.host.shutdown()

        # for eid, entity in self.entities.iteritems():
        #     # Note: you can not delete entity in tick.
        #     # you may cache delete items and delete in next frame
        #     # or just use items.
        #     entity.tick()

        return


def robot_thread():
    while True:
        time.sleep(0.2)
        RobotManager.tick()


if __name__ == '__main__':
    from threading import Thread

    t = Thread(target=robot_thread)
    t.start()
    Server = SimpleServer()
    Server.host.host = "127.0.0.1"
    Server.host.startup(2000)
    while 1:
        # time.sleep(0.2)
        Server.tick()
    # TimerManager.addRepeatTimer(0.1, Server.tick)
    # TimerManager.addRepeatTimer(0.2, RobotManager.tick)
    # while True:
    #     TimerManager.scheduler()
