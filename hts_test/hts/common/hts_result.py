from hts.common.decorator import singleton

@singleton
class HTSResult:
    result_dic = dict()

    def set_result(self, key, data, status_code=None):
        data_item = self._get_data_item(key)
        data_item['data'] = data
        data_item['is_fin'] = True
        if status_code:
            data_item['status_code'] = status_code
        return True

    def get_status(self, key):
        return self._get_data_item(key)['status_code']

    def set_status(self, key, status_code):
        self._get_data_item(key)['status_code'] = status_code
        return True

    def is_fin(self, key):
        return self._get_data_item(key)['is_fin']

    def get_result(self, key):
        return self._get_data_item(key)['data']

    def _get_data_item(self, key):
        data_item = self.result_dic.get(key, None)
        if not data_item:
            self.result_dic[key] = {'data': None, 'is_fin': False, 'status_code': 0}
            data_item = self.result_dic[key]
        return data_item

