from django.conf import settings
from django.urls import path, include
from .views import RequestViewSet

urlpatterns = [
    path('account/list', RequestViewSet.as_view({'get': 'account_list'}), name="account_view"),
    path('account/count', RequestViewSet.as_view({'get': 'account_count'}), name="account_count_view"),
    path('account/info', RequestViewSet.as_view({'get': 'account_info_all'}), name="account_info_all_view"),
    path('account/foreign_info', RequestViewSet.as_view({'get': 'account_foreign_info_all'}), name="account_info_all_view"),
    path('account/info/<account>', RequestViewSet.as_view({'get': 'account_info'}), name="account_info_view"),
    path('account/foreign_info/<account>', RequestViewSet.as_view({'get': 'account_foreign_info'}), name="account_foreign_info_view"),
    path('account/change/<account>/all', RequestViewSet.as_view({'get': 'account_change_all'}), name="account_change_all_view"),
]
