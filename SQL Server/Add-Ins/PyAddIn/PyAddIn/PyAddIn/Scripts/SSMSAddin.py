import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("EnvDTE")
clr.AddReference("EnvDTE80")
import System
from EnvDTE import *
from EnvDTE80 import *
from System.Windows.Forms import *
from System.IO import *
from System.Text import *
from System import Array
from System.Reflection import *
from System.Text.RegularExpressions import *

    

try:
    from SSMSSampleCommand import *
    from SSMSCore import *
    from SSMSProcedureNavigator import *
    from SSMSFormattingCommands import *
except System.Exception, e:
    print "Error importing commands:" + repr(e)


class DefaultAddIn(SqlAddIn):
    def InternalHookEvents(self, Control):
        
        commandBar = self.CreateCommandBar("IronPython")
        self._sqlNavigator = SqlNavigator(self, commandBar)
        FormattingCommands(self, commandBar)
        sampleCommand = SampleCommand(self, commandBar,  "IronPythonSampleCommand", "Sample Command", "Sample Command ToolTip", 13)       
        sampleCommand.GetCommandInstance().Bindings = "Global::ctrl+Shift+F12";       
        
    def OnStartupComplete(self, custom):
        self._sqlNavigator.CreateNavigator.CreateNavigator()
        self._sqlNavigator.CreateNavigatorRefresh.Refresh()

        

def CreateDefaultConsumer():
    return DefaultAddIn()


