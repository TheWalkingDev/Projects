#print "Starting the default script"
import clr
clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import *
from System import *
from System.Text.RegularExpressions import *

def LogMessage(message):    
    print String.Format("{0:HH:MM:ss:fff}: {1}", DateTime.Now, message)
    
    
#print "Test Script starting..."
class BaseHandler:
        
    def OnInit(self, processContext, args):
        self._processContext = processContext
        self._commandArgs = args
        pass
        
    def OnError(self, message):
        pass
        
    def OnMessage(self, message):
        pass        
        
    def OnCommandStarting(self):
        pass        
        
        
    def OnCommandCompleted(self):
        pass        
        
    def Grep(self, input, pattern):
        #print "matching: %s pattern: %s" % (input, pattern)
        return Regex(pattern).Match(input)
    
    def GetStdOut(self):
        return self._processContext.ProcessContext.StandardOutput.GetValue()

    def GetErrorOut(self):
        return self._processContext.ProcessContext.ErrorOutput.GetValue()
    
    def GrepStdOut(self, pattern):
        return self.Grep(self._processContext.ProcessContext.StandardOutput.GetValue(), pattern)


    def GrepStdOut(self, pattern):
        
        return self.Grep(self._processContext.ProcessContext.ErrorOutput.GetValue(), pattern)


def CreateHandler():
    return BaseHandler()