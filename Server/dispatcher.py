# -*- coding: GBK -*-

class Service(object):
    def __init__(self, sid=0):
        super(Service, self).__init__()

        self.service_id = sid
        self.__command_map = {}

    def handle(self, msg, owner):
        cid = msg.cid
        if not cid in self.__command_map:
            raise Exception('bad command %s' % cid)

        f = self.__command_map[cid]
        return f(msg, owner)

    def register(self, cid, function):
        self.__command_map[cid] = function

    def registers(self, command_dict):
        self.__command_map = {}
        for cid in command_dict:
            self.register(cid, command_dict[cid])


class Dispatcher(object):
    def __init__(self):
        super(Dispatcher, self).__init__()

        self.__service_map = {}

    def dispatch(self, msg, owner):
        sid = msg.sid
        if not sid in self.__service_map:
            raise Exception('bad service %d' % sid)

        svc = self.__service_map[sid]
        return svc.handle(msg, owner)

    def register(self, sid, svc):
        self.__service_map[sid] = svc
