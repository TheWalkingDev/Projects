#print "Test Script starting..."
class Handler(BaseHandler):
    def __init__(self):
        #print "Constructor init"
        pass
       
    """ 
    def OnInit(self, processContext):
        #print "Handler init"
        self._processContext = processContext
        self._showCheckin = False
    """
        
    def OnError(self, message):
        print "OnError: " + message;
        pass
        
    def OnMessage(self, message):
        #print "defaultmessage: " + message
        #self._processContext.CreateSpawnedProcess("cmd", "/c echo it works", None, SubHandler()).Start()
        self._showCheckin = True
        #print "CHECKIN ENABLED"
        
        
    def OnCommandStarting(self):
        #print "Command is Starting"
        #self._processContext.CreateSpawnedProcess("notepad.exe", "test", None, SubHandler()).Start()
        pass
        
        
    def OnCommandCompleted(self):
        output = self._processContext.ProcessContext.StandardOutput.GetValue()
        match = self.Grep(output, r"\(current\)")
        if (match.Success):
            print "working directory is current"
            self._processContext.CreateSpawnedProcess("cmd", r"/c echo pushing...", None, SubHandler()).Start()
            print output
            
        else:
            print "grep failed for: (%s)" % (output)
            
            self._processContext.CreateSpawnedProcess("cmd", r"//c echo pushing...", None, SubHandler()).Start()
            #self._processContext.CreateSpawnedProcess("hgtk", "update", None, SubHandler()).Start()
		        
        
   
        return
        
        if self._showCheckin == True:
            #self._processContext.CreateSpawnedProcess("hgtk", "commit", r"d:\Dev\Openbox\Change Controls\GBS Reforecasts Integration", SubHandler()).Start()
            self._processContext.CreateSpawnedProcess("hgtk", "commit", None, SubHandler()).Start()
            print "MUST DO CHECKIN"
        else:
            print "no checkin needed"
        print "Done"
        
class SubHandler(BaseHandler):
    def OnMessage(self, message):
        #print "SubHandler received message:" + message
        pass
        
    def OnCommandStarting(self):
        #print "Subhandler: Command is Starting"
        pass
        
    def OnCommandCompleted(self):
        print "sub handler completed"
        i = 1
        while True:            
            arg = "remote%d" % i
            if self._commandArgs.HasSwitch(arg):
                print "sub: remote:" + arg
            else:
                break;
            i += 1        
        pass
        
            

def CreateHandler():
    return Handler()