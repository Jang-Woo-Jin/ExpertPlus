from django.conf import settings
import ctypes, sys


def is_admin():
    try:
        return ctypes.windll.Shell32.IsUserAnAdmin()

    except:
        return False




