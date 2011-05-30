from SSMSCore import *
instanceId = 61


class FirstCommandHandler(CommandHandler): 
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        self._DTE.ActiveDocument.Selection.SelectAll()
        text = self._DTE.ActiveDocument.Selection.Text
        self._DTE.ActiveDocument.Selection.Text += "\nadd some text\n" + text
        #ShowMessage(text)
        #self._DTE.ExecuteCommand("File.Close")
        
  
        
class Test01Collapse(CommandHandler): 
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        """
        Sub CollapseExample()
          ' This generates a text document listing all available command names.
          Dim Cmd As Command
          Dim PrjItem As ProjectItem
          Dim Doc As Document
          Dim TxtDoc As TextDocument
          DTE.ItemOperations.NewFile ("General\Text File")
          Set Doc = ActiveDocument
          Set TxtDoc = Doc.Object("TextDocument")
          For Each Cmd In Commands
          If (Cmd.Name <> "") Then
            TxtDoc.Selection.Text = Cmd.Name & vbLF
            TxtDoc.Selection.Collapse
          End If
          Next
        End Sub
        
        """
        #self._DTE.DTE.ItemOperations.NewFile ("Fil")
        #self._DTE.ExecuteCommand("File.New")
        self.DTENewQuery()
        Doc = self._DTE.ActiveDocument
        TxtDoc = Doc.Object("TextDocument")
        commands = self._sqlAddin.findCommands("")
        commandText = ""
        
        for Cmd in self._DTE.Commands:
            if (Cmd.Name <> ""):
                commandText += Cmd.Name + "\n"
                
        #TxtDoc.Selection.Text = commandText
        #TxtDoc.Selection.Collapse
        #self._DTE.ActiveDocument.Selection.Collapse()
        self._DTE.ActiveDocument.Selection.Insert(commandText)

def fdir(obj):
    l = dir(obj)
    l.sort()
    return filter(lambda item: not item.StartsWith("__"), l)

class Test02ExecQuery(CommandHandler): 
    def TestExecQuery(self):
        self._DTE.ActiveDocument.Selection.SelectAll()
        self._DTE.ActiveDocument.Selection.Insert( "SELECT GETDATE()")
        commands = self._sqlAddin.findCommands("QueryDesigner.ExecuteSQL")
        if commands.Count > 0:
            #commands[0].Exec()
            #ShowMessage(repr(dir(commands[0])))
            pass
        
        self._DTE.ExecuteCommand("Query.Execute")
    def TestChangeConnection(self):
        self._DTE.ExecuteCommand("Query.ChangeConnection")
        
    def TestEnumDocuments(self):
        documents = self._sqlAddin.findDocuments()
        for doc in documents:
            ShowMessage(doc.FullName)
    

    
            
    def TestToolWindow(self):
        #ShowMessage(repr(dir(self._sqlAddin)))
        window = self._sqlAddin.CreateToolWindow(
            "MyAddinUI.UIControl1",
            r"Z:\Dev\Play\C#\SQLAddIn\MyAddin1\MyAddin1\bin\Debug\MyAddinUI.dll",
            "4c410c93-d66b-495a-9de2-99d5bde4a3b9",
            "My Test Tool Window")
            
            
        #window.Window.Height = 500;
        #window.Window.Width = 400;
        window.ControlObject.WriteLog(GetMessage("It worked"))
        #ShowMessage(window.ControlObject.Text)
        
    def TestReflect(self):
        #self.TestChangeConnection()
        #self.TestEnumDocuments()
        #self.TestToolWindow()
        #d = fdir(self._DTE.Solution)
        d = self._DTE.Windows
        
        for m in d:          
            if m.Caption == "Object Explorer":
                self._sqlAddin.ShowMessage(repr(m.ObjectKind))
                self._sqlAddin.ShowMessage(repr(m))
        #for win in self._DTE.Events:
        #    log("window: " + win.Caption)                        
     
    def Provider_SelectionChanged(sender, args):
        self._sqlAddin.ShowMessage("Provider_SelectionChanged")
        
    def InternalExecute(self, ExecuteOption, VariantIn, VariantOut, Handled):
        vsDll = r"c:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Microsoft.SqlServer.SqlTools.VSIntegration.dll"
        clr.AddReferenceToFileAndPath (vsDll)         
        from Microsoft.SqlServer.Management.UI.VSIntegration import *
        #objectExplorer = ServiceCache.GetObjectExplorer();
        self._sqlAddin.ShowMessage("ServiceCache:" + repr(fdir(ServiceCache)))
        #self._sqlAddin.ShowMessage("Query: ServiceCache.ScriptFactory")
        #self._sqlAddin.ShowMessage("ServiceCache.ScriptFactory:" + repr(ServiceCache.ScriptFactory))
        #self._sqlAddin.ShowMessage("Queried: ServiceCache.ScriptFactory")
        
        scriptType = Editors.ScriptType.Sql
        self._sqlAddin.ShowMessage("Creating black script...")
        ServiceCache.ScriptFactory.CreateNewBlankScript(scriptType)
        self._sqlAddin.ShowMessage("Created blank script")

        self._sqlAddin.ShowMessage("ServiceCache.ServiceProvider:" + repr(fdir(ServiceCache.ServiceProvider)))
        self._sqlAddin.ShowMessage("ServiceCache.GetObjectExplorer:" + repr(ServiceCache.ServiceProvider.GetObjectExplorer()))
        self._sqlAddin.ShowMessage("ServiceCache.GlobalConnectionInfo:" + repr(fdir(ServiceCache.GlobalConnectionInfo)))
        self._sqlAddin.ShowMessage("ServiceCache.VSMonitorSelection:" + repr(fdir(ServiceCache.VSMonitorSelection)))
        
        self._sqlAddin.ShowMessage("ServiceCache.VsServiceProvider:" + repr(fdir(ServiceCache.VsServiceProvider)))
        self._sqlAddin.ShowMessage("ServiceCache.VsShell:" + repr(fdir(ServiceCache.VsShell)))
        self._sqlAddin.ShowMessage("ServiceCache.VsUIShell:" + repr(fdir(ServiceCache.VsUIShell)))
        #objExplorer = ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
        #provider = objectExplorer.GetService(typeof(IObjectExplorerEventProvider))
        
        #SelectionChanged += Provider_SelectionChanged
        self._sqlAddin.ShowMessage("Internal execute")
        
       
              
        
        #self._sqlAddin.Control.UIController.CreateAddinWindow(self._sqlAddin.Control.AddInInstance.DTE, self._sqlAddin.Control.AddInInstance)
        pass
        
