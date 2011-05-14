class Handler(BaseHandler):       
    def OnInit(self, processContext):
        BaseHandler.OnInit(self, processContext)
        self._showCheckin = False
        
    def OnError(self, message):
        print "OnError: " + message;
        pass
        
    def OnMessage(self, message):
        # currently anything from the status window means checkin
        self._showCheckin = True       
        
    def OnCommandStarting(self):
        pass
        
        
    def OnCommandCompleted(self):
        if self._showCheckin == True:
            print "Checkin REQUIRED"
            self._processContext.CreateSpawnedProcess("hgtk", "commit", None, SubHandler()).Start()
            self._processContext.CreateSpawnedProcess("hgtk", "log", None, SubHandler()).Start()
        else:
            print "Checkin not required"
        print "Done"
        
class SubHandler(BaseHandler):
    def OnMessage(self, message):
        pass
        
    def OnCommandStarting(self):
        pass
        
    def OnCommandCompleted(self):
        pass
        
            

def CreateHandler():
    return Handler()