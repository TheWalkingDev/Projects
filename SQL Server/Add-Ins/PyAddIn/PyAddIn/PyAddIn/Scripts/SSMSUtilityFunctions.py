import clr
clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import *
from System.IO import *
from System.Text import *
from EnvDTE import *
from EnvDTE80 import *
from System import Array
import System
from System.Text.RegularExpressions import *

instanceId = 1

         
           
def GetMessage(message):
    return "[%d][%s]: %s" % (instanceId,  System.DateTime.Now.ToString("HH:MM:ss"), message)    

def ShowMessage(message):
    MessageBox.Show(GetMessage(message))


def ShowFuncParams(func):
    try:
        import sys
        sys.path.append(r"c:\Program Files (x86)\IronPython 2.7\Lib")
        import inspect
        args, varargs, varkw, defaults = inspect.getargspec(func)
        ShowMessage(repr(args))
    except System.Exception, e:
        ShowMessage("Error importing inspect:" + repr(e)) 
             
        
DebuggerEnabled = True           
def InspectWithPyPad(contextGlobals, contextLocals):
    #if not DebuggerEnabled:
    #    return
	import clr
	clr.AddReference("ACSR.Controls.ThirdParty.dll")
	from ACSR.Controls.ThirdParty.Python import *
	inspect = PyInspector()
	for key in contextLocals.keys():
		inspect.SetVariable(key, contextLocals[key])
	for key in contextGlobals.keys():
		inspect.SetVariable(key, contextGlobals[key])
	inspect.Inspect()
     
    
def ReadLines(fileName):
    f = FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    try:
        sw = StreamReader(f)
        try:
            while not sw.EndOfStream:
                line = sw.ReadLine()
                yield line
        finally:
            sw.Close()
    finally:
        f.Close()
    
    