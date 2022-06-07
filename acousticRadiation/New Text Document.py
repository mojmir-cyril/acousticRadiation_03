import clr

class MapLaterCall(object):
    calls = []
    msgColor = "#779977"
    doLog = False

    @staticmethod
    def Log(msg):
        if MapLaterCall.doLog:
            try:
                MCD.Log(msg)  # , color = MapLaterCall.msgColor
            except:
                Msg(msg)

    @staticmethod
    def Wrn(msg):
        try:
            MCD.Wrn(msg)
        except:
            Wrn(msg)

    def __init__(self, **k):
        """
            def Ahoj(a = 5): return a
            MapLaterCall(func = Ahoj)
            MapLaterCall(func = Ahoj, interval = 200)
            MapLaterCall(func = Ahoj, args = [15])
            MapLaterCall(func = Ahoj, kwar = {'a': 35})
            MapLaterCall(func = Ahoj, kwar = {'a': 35}, maxIgnore = 100)
            MapLaterCall(func = Ahoj, kwar = {'a': 35}, maxIgnore = 100, waitForLastCall = False)
            MapLaterCall(func = Ahoj, note = "a note written to the Log when call")
            -----------------
            global a
            a = 5
            def F():
                global a
                Msg("a == 3: {}".format(a == 3))
                return a == 3
            def A():
                Msg("xxx")
            MapLaterCall(func = A, ifFunc = F, maxRepeat = 100, interval = 1000)
            -----------------
            def R(a = 5): raise Exception("R()")
            MapLaterCall(func = R)
        """

        class Call(object):
            def __init__(self, func=None, interval=200, args=[], kwar={}, maxIgnore=0,
                         waitForLastCall=True,
                         note=None, retFunc=None,
                         ifFunc=None, maxRepeat=1):
                try:
                    MapLaterCall.Log(
                        "MapLaterCall.Call.__init__(): {}\n - args: {}\n - kwar: {}".format(func.__name__, args,
                                                                                            kwar))  # , color = MapLaterCall.msgColor
                    self.func = func
                    self.args = args
                    self.kwar = kwar
                    # --
                    self.maxIgnore = maxIgnore
                    self.igns = 0
                    # --
                    self.waitForLastCall = waitForLastCall
                    self.note = note
                    self.retFunc = retFunc
                    # --
                    self.ifFunc = ifFunc
                    self.repeats = 0
                    self.maxRepeat = maxRepeat
                    # --
                    try:
                        self.timer = aTK.Timer()
                    except:
                        clr.AddReference("Ans.UI.Toolkit"); import Ansys.UI.Toolkit as aTK; self.timer = aTK.Timer()
                    self.timer.Tick += self.__Call
                    self.timer.Interval = interval
                    self.timer.Start()
                except Exception as err:
                    Err("MapLaterCall.Call.__init__(): {}. Timer did not be started.".format(err))

            def __Call(self, *args):
                try:
                    if self.ifFunc != None:
                        self.repeats += 1
                        if self.repeats > self.maxRepeat:
                            MapLaterCall.Log("MapLaterCall.__Call(): self.repeats >= self.maxRepeat ----> stop ...")
                            self.timer.Stop()
                            return
                        if not self.ifFunc():
                            MapLaterCall.Log("MapLaterCall.__Call(): self.ifFunc = False ----> waiting ...")
                            return
                    MapLaterCall.Log(
                        "MapLaterCall.Call.__Call(): {} (note: {})\n - args: {}\n - kwar: {}\n - stopping the timer ...".format(
                            self.func.__name__, self.note, self.args, self.kwar))
                    self.timer.Stop()
                    try:
                        MapLaterCall.calls.remove(self)
                    except:
                        MapLaterCall.Wrn(
                            "MapLaterCall.Call.__Call(): A problem to remove 'self' with funtion name '{}' from 'MapLaterCall.calls'. ".format(
                                self.func.__name__))
                    try:
                        ret = self.func(*self.args, **self.kwar)
                        if self.retFunc != None: self.retFunc(*ret)
                    except Exception as err:
                        Err("MapLaterCall.Call.__Call(self.func.__name__ = {}): Exception during calling: {}".format(
                            self.func.__name__, err))
                except Exception as err:
                    Err("MapLaterCall.Call.__Call(): " + str(err))

        try:
            MapLaterCall.Log("MapLaterCall.__init__(): func.__name__ = " + str(k["func"].__name__))
            for c in MapLaterCall.calls:
                if c.func == k["func"] and c.args == k.get("args", []) and c.kwar == k.get("kwar", {}):
                    c.igns += 1
                    maxIgnore = k.get("maxIgnore", c.maxIgnore)
                    if c.igns <= maxIgnore:
                        waitForLastCall = k.get("waitForLastCall", c.waitForLastCall)
                        if waitForLastCall:
                            # reseting interval:
                            MapLaterCall.Log(
                                "MapLaterCall.__init__(): Found same. Interval reseted. (waitForLastCall = True)")
                            c.timer.Stop()
                            c.timer.Start()
                        return MapLaterCall.Log(
                            "MapLaterCall.__init__(): Found same. Call ignored (igns: {}, maxIgnore: {})!!!".format(
                                c.igns, maxIgnore))
            MapLaterCall.Log("MapLaterCall.__init__(): MapLaterCall.calls += [Call(**k)]\n - k: " + str(k))
            MapLaterCall.calls += [Call(**k)]
        except Exception as err:
            Err("MapLaterCall.__init__(): " + str(err))

