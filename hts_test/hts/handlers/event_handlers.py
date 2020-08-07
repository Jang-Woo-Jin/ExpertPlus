import win32com.client
from pythoncom import PumpWaitingMessages, Empty, CoInitialize
import time

from hts.common import utils
from hts.common.hts_result import HTSResult


class BaseEventHandler:
    is_connect = False
    client = None
    hts_result = None

    def __init__(self):
        if utils.is_admin():
            CoInitialize()
            self.yfValues = win32com.client.Dispatch('YFExpertPlus.YFValues')
            self.yfValuesList = win32com.client.Dispatch('YFExpertPlus.YFValueList')
            self.hts_result = HTSResult()
        else:
            raise ValueError('Not Admin')


class RequestEventHandler(BaseEventHandler):
    def OnInitialize(self, *args, **kwargs):
        print('OnInitialize called.', self.client)
        self.is_connect = True
        return True

    def OnStatus(self, Status=Empty, TrCode=Empty, MsgCode=Empty, MsgName=Empty):
        status_string = f'Status: {Status} - {TrCode}, {MsgCode}, {MsgName}'
        print(status_string)
        self.hts_result.set_result(TrCode, status_string, Status)
        return True

    def OnReceiveData(self,
                      TrCode=Empty,
                      Value=Empty,
                      ValueList=Empty,
                      NextFlag=Empty,
                      SelectCount=Empty,
                      MsgCode=Empty,
                      MsgName=Empty):

        print(
            f'ReceiveData - TrCode: {TrCode}, MsgCode: {MsgCode}, MsgName: {MsgName}, Value: {Value}, ValueList: {ValueList}')

        if self.hts_result.get_status(TrCode) in [1, 2]:
            self.hts_result.set_status(TrCode, 4)

        else:
            value_flag = False
            value_list_flag = False

            if Value:
                self.yfValues.SetValueData(self.client.GetKorValueHeader(TrCode), Value)
                value_flag = True

            if ValueList:
                self.yfValuesList.SetListData(self.client.GetKorValueListHeader(TrCode), ValueList, SelectCount-1)
                value_list_flag = True

            return_data = self._serialize_data(TrCode, value_flag=value_flag, value_list_flag=value_list_flag)
            self.hts_result.set_result(TrCode, return_data)

        return True

    def _serialize_data(self, TrCode, value_flag=False, value_list_flag=False):
        _data = dict()
        if value_flag:
            for i in range(len(self.client.GetValueHeader(TrCode).split(';'))):
                _data[self.yfValues.GetName(i)] = self.yfValues.GetValue(i)

        if value_list_flag:
            print(f'ValueList - RowCount: {self.yfValuesList.RowCount()}, ColCount: {self.yfValuesList.ColCount()}')

            column_name_list = []
            for i in range(self.yfValuesList.ColCount()):
                column_name_list.append(self.yfValuesList.GetColName(i))

            list_data = []
            self.yfValuesList.RowFirst()
            for i in range(self.yfValuesList.RowCount()):
                list_item = dict()
                for j in range(self.yfValuesList.ColCount()):
                    list_item[column_name_list[j]] = self.yfValuesList.GetRowDataCell(j)

                list_data.append(list_item)
                self.yfValuesList.RowNext()
            _data['list_data'] = list_data
        return _data
