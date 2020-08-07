from django.shortcuts import render
from rest_framework.response import Response
from rest_framework.viewsets import ModelViewSet, ViewSet

from django.conf import settings
from hts.common import utils
from hts.handlers.hts_handlers import RequestDataHandler
from multiprocessing import Process, Queue


class RequestViewSet(ViewSet):
    master_pw = 'fount2015'
    master_fee_acount = '26859685901'

    def account_list(self, request):
        account_list = self._get_account_list()
        return Response({'count': len(account_list),
                         'accounts': account_list})

    def account_count(self, request):
        request_data_handler = self._get_request_data_handler()
        account_count = request_data_handler.get_account_count()
        return Response({'count': account_count})

    def account_info(self, request, account):
        request_data_handler = self._get_request_data_handler()
        response = request_data_handler.get_account_info(account, self.master_pw)
        return Response({'account': account, 'response': response})

    def account_foreign_info(self, request, account):
        request_data_handler = self._get_request_data_handler()
        response = request_data_handler.get_account_info(account, self.master_pw, 1)
        return Response({'account': account, 'response': response})

    def account_info_all(self, request):
        request_data_handler = self._get_request_data_handler()
        account_list = self._get_account_list()
        result = dict()
        for account in account_list:
            account_info = request_data_handler.get_account_info(account, self.master_pw)
            result[account] = account_info
        return Response({'account_infos': result})

    def account_foreign_info_all(self, request):
        request_data_handler = self._get_request_data_handler()
        account_list = self._get_account_list()
        result = dict()
        for account in account_list:
            account_info = request_data_handler.get_account_info(account, self.master_pw, 1)
            result[account] = account_info
        return Response({'account_infos': result})

    def account_change_all(self, request, account):
        request_data_handler = self._get_request_data_handler()
        account_change_info = request_data_handler.get_account_change_all(account, self.master_pw)

        return Response({'account_change_info': account_change_info})

    def _get_account_list(self):
        request_data_handler = self._get_request_data_handler()
        account_list = request_data_handler.get_account_list()
        if self.master_fee_acount in account_list:
            account_list.remove(self.master_fee_acount)
        return account_list

    def _get_request_data_handler(self):
        return RequestDataHandler()
