import datetime
import sys

msg = ExtAPI.Log.WriteMessage
wrn = ExtAPI.Log.WriteWarning
err = ExtAPI.Log.WriteError

wrn("decorators")

def printException():
    exception_type, exception_object, exception_traceback = sys.exc_info()
    filename = exception_traceback.tb_frame.f_code.co_filename
    line_number = exception_traceback.tb_lineno
    err("Exception: {}, in file {} on line {}.".format(exception_object, filename, line_number))

def callback(func):
    def __wrapper(*args, **kwargs):
        try:
            now = datetime.datetime.now()
            # msg("before")
            retVal = func(*args, **kwargs)
            # msg("after")
        except Exception as e:
            err("Callback error in function: {name}".format(name=func.__name__))
            printException()
            return None
        duration = (datetime.datetime.now() - now)
        maxDuration = datetime.timedelta(microseconds=1000000)
        if duration > maxDuration:
            wrn("Callback: {name}".format(name=func.__name__))
            wrn("... elapsed time is {}.{} seconds.".format(duration.seconds, str(duration.microseconds)[0:2]))
        return retVal
    return __wrapper

def forwardError(func):
    def __wrapper(*args, **kwargs):
        try:
            now = datetime.datetime.now()
            # msg("before")
            retVal = func(*args, **kwargs)
            # msg("after")
        except Exception as e:
            err("Callback forward error in function: {name}".format(name=func.__name__))
            printException()
            raise Exception()
        duration = (datetime.datetime.now() - now)
        maxDuration = datetime.timedelta(microseconds=1000000)
        if duration > maxDuration:
            wrn("Callback: {name}".format(name=func.__name__))
            wrn("... elapsed time is {}.{} seconds.".format(duration.seconds, str(duration.microseconds)[0:2]))
        return retVal
    return __wrapper

print("ahoj")