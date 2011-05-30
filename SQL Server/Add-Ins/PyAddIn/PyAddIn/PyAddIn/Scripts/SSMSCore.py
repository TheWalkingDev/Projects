import clr
clr.AddReference("System.Windows.Forms")
clr.AddReference("EnvDTE")
clr.AddReference("EnvDTE80")
from System.Windows.Forms import *
from System.IO import *
from System.Text import *
from EnvDTE import *
from EnvDTE80 import *
from System import Array
import System
from SSMSUtilityFunctions import *


    
        
#DTE: http://msdn.microsoft.com/en-us/library/aa301306%28v=vs.71%29.aspx
#ActiveDocument: http://msdn.microsoft.com/en-us/library/aa301303%28v=vs.71%29.aspx




class SqlAddIn:        
    def __init__(self):
        self._menuCommandBar = None
        self._commandMap = {}        
        
    def InternalShowMessage(self, message):
        ShowMessage(message)
        
    def PrintMessage(self, message):
        if self.Adapter:
            self.Adapter.LogMessage(message)
            return True
        else:
            return False
        
    def ShowMessage(self, message):
        if not self.PrintMessage(message):
            self.InternalShowMessage(GetMessage(message))
        
       
    def getCommandBars(self):
        return self.Adapter.AddInInstance.DTE.CommandBars
    def getCommands(self):
        return self.Adapter.AddInInstance.DTE.Commands
                
    def getMenuCommandBar(self):
        if not self._menuCommandBar:
            self._menuCommandBar = self.getCommandBars()["MenuBar"]
        return self._menuCommandBar
        
    def getToolsMenuControl(self):
        return self.getMenuCommandBar().Controls["Tools"]
    
    def findCommands(self, CommandName):
        commandItems = []
        for cmd in self.Adapter.AddInInstance.DTE.Commands:
            if cmd.Name.ToString().Contains(CommandName):
                commandItems.append(cmd)
        return commandItems
    def findDocuments(self, Name=None):
        results = []
        for doc in  self.Adapter.AddInInstance.DTE.Documents:    
            if not Name or doc.Name.ToString().Contains(Name):
                results.append(doc)
        return results
        
    def removeCommands(self, CommandName):
        commandItems = self.findCommands(CommandName)
        for cmd in commandItems:
            cmd.Delete()
            
    def findCommandBarControls(self, menu, commandBarName):
        menuItems = []
        for control in menu.CommandBar.Controls:
            if control.Caption.ToString().Contains(commandBarName):                
                menuItems.append(control)                
        return menuItems
    
    
    def removeCommandBarControls(self, menu, commandBarName):
        for control in self.findCommandBarControls(menu, commandBarName):
            control.Delete()
    
    def addCommandBar(self, Menu, Name):
        return self.getCommands().AddCommandBar(Name, 
                vsCommandBarType.vsCommandBarTypeMenu, Menu.CommandBar)
    def reAddCommandBar(self, Menu, Name):
        self.removeCommandBarControls(Menu, Name)
        return self.addCommandBar(Menu, Name)
      
    def addCommand(self, Bar, Name, Text, ToolTip, ImageIndex, Handler):
        self.removeCommands(Name)            
        cmd = self.Adapter.AddNamedCommand(Name, Text, ToolTip, 13)
        cmd.AddControl(Bar, 1)
        #cmd.Bindings = "Global::ctrl+F2"
        self._commandMap[cmd.Name] = Handler
        return cmd
        
    def CreateCommandBar(self, MenuName):
        try:
            commandBar = self.reAddCommandBar(self.getToolsMenuControl(), MenuName)
            return commandBar
        except System.Exception, e:
            self.ShowMessage("Error in CreateCommands:" + repr(e))              
        
            
    def InternalHookEvents(self, Control):            
        pass
    def HookEvents(self, adapter):
        try:
            self.Adapter = adapter
            self._DTE = adapter.AddInInstance.DTE            
            self.InternalHookEvents(adapter)
            #self.PrintMessage("Hooked Events")
        except System.Exception, e:
            self.ShowMessage("Error in HookEvents:" + repr(e))              
        
    def InternalUnHookEvents(self, Control):            
        pass

    def UnhookEvents(self, Control):
        try:
            self.InternalUnHookEvents(Control)
        except System.Exception, e:
            self.ShowMessage("Error in UnhookEvents:" + repr(e))              
            
    def OnConnection(self, adapter):
        #self.PrintMessage("OnConnection complete")
        pass

    def OnDisconnection(self, disconnectMode, custom):
        pass
        

    def OnAddInsUpdate(self, custom):
        pass
            
    def OnStartupComplete(self, custom):
        pass
        
    def OnBeginShutdown(self, custom):
        pass

    def QueryStatus(self, CmdName, NeededText, StatusOption, CommandText):
        return (vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled, "TestCommandText")
        pass
        
        
    def CommandEvents_BeforeExecute (self, Guid, ID, CustomIn, CustomOut, CancelDefault):
        self.PrintMessage("CommandEvents_BeforeExecute succesful: %s %s" % (Guid, ID))

    def CommandEvents_AfterExecute (self, Guid, ID, CustomIn, CustomOut):        
        self.PrintMessage("CommandEvents_AfterExecute succesful: %s %s" % (Guid, ID))


    def Exec(self, CmdName, ExecuteOption, VariantIn, VariantOut, Handled):
        try:
            if self._commandMap.has_key(CmdName):            
                cmd = self._commandMap[CmdName]
                cmd.Execute(ExecuteOption, VariantIn, VariantOut, Handled)
            else:
                ShowMessage("Cannot find command: %s\n%s" % (CmdName, repr(_commandMap)))
                
        except System.Exception, e:
            ShowMessage("Error in HookEvents:" + repr(e))     
    
    def FindWindow(self, caption):
        for win in self.Adapter.AddInInstance.DTE.Windows:
            if caption.CompareTo(win.Caption) == 0:
                return win
        return None
    
    def CreateHostWindow(self, control, caption, guid):
        try:
            win = self.FindWindow(caption)
            if (win):
                self.Adapter.HostWindow(win.Object, control)
                win.Visible = True
                self.PrintMessage("Created HostWindow:: %s, %s. Existing window found, reusing." % (caption, guid)) 
            else:            
                win = self.Adapter.CreateHostWindow(control, caption, guid)
                win.Window.Visible = True
                self.PrintMessage("Created HostWindow:: %s, %s" % (caption, guid)) 
        except System.Exception, e:
            self.ShowMessage("Error in CreateToolWindow(%s, %s, %s): %s" % (TypeName, AssemblyPath, GuidStr, repr(e)))              
        return win
    
    def CreateToolWindow(self, TypeName, AssemblyPath, GuidStr, Caption):
        try:
            clr.AddReferenceToFileAndPath (AssemblyPath)                                 
            assemblyFileName = Path.GetFileName(AssemblyPath)
            asm = Assembly.Load(assemblyFileName)        
            id = System.Guid(GuidStr)
            sGuid = "{%s}" % id.ToString()
            window =  self.Control.UIController.CreateToolWindow(TypeName, 
                asm.Location, 
                Caption,
                id, 
                self.Control.AddInInstance.DTE, 
                self.Control.AddInInstance)
        except System.Exception, e:
            self.ShowMessage("Error in CreateToolWindow(%s, %s, %s): %s" % (TypeName, AssemblyPath, GuidStr, repr(e)))              
        self.PrintMessage("Created toolCreateToolWindow(%s, %s, %s)" % (TypeName, AssemblyPath, GuidStr)) 
        return window


class CommandHandler:
    def __init__(self, SqlAddIn, ACommandBar, Name, Text, ToolTip, ImageIndex):        
        self._commandInstance = SqlAddIn.addCommand(ACommandBar, Name, Text, ToolTip, ImageIndex, self)
        self._sqlAddin = SqlAddIn
        self._DTE = self._sqlAddin.Adapter.AddInInstance.DTE
        self._Adapter = self._sqlAddin.Adapter
    def GetWorkingDirectory(self):
        return Path.GetDirectoryName(self._Adapter.CommonUIAssemblyLocation)
    def GetCommandInstance(self):
        return self._commandInstance    
    def ShowMessage(self, message):
        self._sqlAddin.ShowMessage(message)
    def GetActiveWindow(self):
        return self._DTE.ActiveWindow
    
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        pass
    def Execute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        try:
            self.InternalExecute(ExecuteOption, VariantIn, VariantOut, Handled)
        except System.Exception, e:
            try:
                self._sqlAddin.ShowMessage("Error in CommandHandler.Execute: %s\n%s" % (e.Message, repr(e)))
            except System.Exception, e:
                ShowMessage("Error in CommandHandler.Execute: %s" (e))
        pass
        
    def DTENewQuery(self):
        self._DTE.ExecuteCommand("File.NewQuery")
        
    def DTEExecuteQuery(self):
        self._DTE.ExecuteCommand("Query.Execute")


class WindowEvents:      
    def __handleWindowActivated(self, gotFocus, lostFocus):
        try:
            self._raiseWindowActivated(gotFocus, lostFocus)
        except:
            pass        
    def InitEvents(self, DTE, windowActivatedEventHandler):
        self._raiseWindowActivated = windowActivatedEventHandler
        DTE.Events.WindowEvents().WindowActivated -= self.__handleWindowActivated
        DTE.Events.WindowEvents().WindowActivated += self.__handleWindowActivated

class DocumentEvents:      
    def __handleSavedEvent(self, document):
        try:
            self.__eventHandler(document)
        except:
            pass
    
    def InitEvents(self, DTE, eventHandler):
        self.__eventHandler = eventHandler
        self._DTE.Events.DocumentEvents().DocumentSaved -= self.__handleSavedEvent
        self._DTE.Events.DocumentEvents().DocumentSaved += self.__handleSavedEvent
        



