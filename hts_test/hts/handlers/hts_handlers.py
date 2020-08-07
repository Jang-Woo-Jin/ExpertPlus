import win32com.client
from pythoncom import PumpWaitingMessages, Empty, CoInitialize
import time

from hts.common import utils
from hts.common.decorator import singleton
from hts.common.hts_result import HTSResult
from .event_handlers import RequestEventHandler


@singleton
class RequestDataHandler:
    handler = None
    event_handler = None
    hts_result = None

    def __init__(self):
        if utils.is_admin():
            CoInitialize()
            self.handler = win32com.client.Dispatch('YFExpertPlus.YFRequestData')
            self.event_handler = win32com.client.WithEvents(self.handler, RequestEventHandler)
            self.event_handler.client = self.handler

            self.handler.ComInit()
            self.handler.GSComInit(0)

            print('Start Init request data handler!')
            while not self.event_handler.is_connect:
                PumpWaitingMessages()
                time.sleep(0.5)

            self.hts_result = HTSResult()
            print('Init request data handler!')

        else:
            raise ValueError('Not Admin')

    def get_account_count(self):
        return self.handler.AccountCount()

    def get_account_list(self):
        account_list = []
        for i in range(self.get_account_count()):
            account_list.append(self.handler.AccountItem(i))
        return account_list

    def get_account_info(self, account_num, password, foreign_flag=0):
        tr_code = 'TA1001'
        self.handler.RequestInit()
        self.handler.SetData("Account", account_num)
        self.handler.SetData("Password", password)

        if foreign_flag:
            tr_code = 'TA6101'
            self.handler.SetData('ExRateAly', '2')
        self.handler.RequestData(tr_code, 0)

        while not self.hts_result.is_fin(tr_code):
            time.sleep(0.5)

        result = self.hts_result.get_result(tr_code)
        return result

    def get_account_change_all(self, account_num, password):
        tr_code = 'TA6102'
        self.handler.RequestInit()
        self.handler.SetData("Account", account_num)
        self.handler.SetData("Password", password)
        self.handler.SetData("QueryType", '1')
        self.handler.SetData("QueryType1", '1')
        self.handler.SetData("QueryType2", '1')
        self.handler.SetData("QueryType3", '1')
        self.handler.SetData("QueryType4", '1')
        self.handler.SetData("QueryType5", '1')
        self.handler.SetData("QueryType6", '1')
        self.handler.SetData("FromDate", '20200701')
        self.handler.SetData("ToDate", '20200801')
        self.handler.SetData("StockNo", '')
        self.handler.RequestData(tr_code, 0)

        while not self.hts_result.is_fin(tr_code):
            time.sleep(0.5)

        result = self.hts_result.get_result(tr_code)
        return result
