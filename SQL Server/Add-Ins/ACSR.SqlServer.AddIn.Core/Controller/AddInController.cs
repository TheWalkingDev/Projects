/*  2008 R2 Object Explorer support.
 * see this blog
   http://blog.sqlblog.com/blogs/jonathan_kehayias/archive/2009/08/22/sql-2008-r2-breaks-ssms-addins.aspx
 you used to be able to hook up to the object explorer but now it just doesnt want to work
 * every interface or code snippet that is suggested no longer works as it either doesnt exist
 * or it is null, so to try again later add these references and uncomment the GetObjectExplorer method
 
     <Reference Include="Microsoft.SqlServer.SqlTools.VSIntegration">
      <HintPath>C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Microsoft.SqlServer.SqlTools.VSIntegration.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.VisualStudio.OLE.Interop, Version=7.1.40304.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.OLE.Interop.dll</HintPath>
    </Reference>
    <Reference Include="ObjectExplorer">
      <HintPath>C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\ObjectExplorer.dll</HintPath>
    </Reference>
    <Reference Include="SqlWorkbench.Interfaces">
      <HintPath>C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\SqlWorkbench.Interfaces.dll</HintPath>
    </Reference>
 * 
 // trying to get this to work only reason its in here is to use IObjectExplorer which doesnt work anymore since R2 so stuck
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.SqlServer.Management.UI.VSIntegration;
 
OR: PASTE This in PyPad
 * 
 import clr
clr.AddReferenceToFileAndPath(r"C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\Microsoft.SqlServer.SqlTools.VSIntegration.dll")
clr.AddReferenceToFileAndPath(r"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.OLE.Interop.dll");
clr.AddReferenceToFileAndPath(r"C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\ObjectExplorer.dll")
clr.AddReferenceToFileAndPath(r"C:\Program Files (x86)\Microsoft SQL Server\100\Tools\Binn\VSShell\Common7\IDE\SqlWorkbench.Interfaces.dll")

from Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer import *
from Microsoft.SqlServer.Management.UI.VSIntegration import *
  
 * and waste some time and then some more time
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using EnvDTE;
using Extensibility;
using PyAddIn.Controller;
using System.IO;
using ACSR.SqlServer.Addin.Core.UI;
using ACSR.Controls.ThirdParty.Python;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ACSR.SqlServer.AddIn.Core.Interface;
using ACSR.PythonScripting;
using System.Text.RegularExpressions;


namespace PyAddIn
{
    public class AddInController : IAddInController
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public static string MessageTypeToString(MessageType msgType)
        {
            switch (msgType)
            {
                case MessageType.Message:
                    return "Message";
                case MessageType.Warning:
                    return "Warning";
                case MessageType.Error:
                    return "Error";
                default:
                    return "Unknown";
            }
        }

        IAddInAdapter _adapter;
        
        public AddInController(IAddInAdapter adapter)
        {
            _adapter = adapter;
            _adapter.OnMessage += new AddInMessageEvent(_adapter_OnMessage);
            _adapter.OnError += new AddInMessageEvent(_adapter_OnError);
            _adapter.OnWarning += new AddInMessageEvent(_adapter_OnWarning);
            _adapter.OnBeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(_adapter_OnBeforeExecute);
            _adapter.OnAfterExecute += new _dispCommandEvents_AfterExecuteEventHandler(_adapter_OnAfterExecute);

            _workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _uiDLL = Path.Combine(_workingDirectory, "ACSR.SqlServer.Addin.Core.UI.dll");
            _adapter.CommonUIAssemblyLocation = _uiDLL;
        }

       

        void _adapter_OnWarning(object sender, string message, MessageType messageType)
        {
            LogWarning(message);
        }

        void _adapter_OnError(object sender, string message, MessageType messageType)
        {
            LogError(message);
        }

        void _adapter_OnMessage(object sender, string message, MessageType messageType)
        {
            LogMessage(message);
        }

        IToolWindowContext _logWindow = null;
        IToolWindowContext _debuggerHostWindow;

        dynamic _logWindowControl = null;
        string _workingDirectory;
        string _uiDLL;

      
        void CreateLogWindow()
        {
            _logWindow = _adapter.CreateCommonUIControl("ACSR.SqlServer.Addin.Core.UI.UcLogWindow", "IronPython LogWindow", new Guid("3ADC13FF-DCF4-4C49-B2EF-3D78DECDC664"));
            _logWindowControl = ((dynamic)_logWindow.ControlObject);
        }
        void InternalLogMessage(string message, MessageType messageType)
        {
            LogMessageToFile(message, messageType);
            LogMessageToWindow(message, messageType);
        }
        void LogWarning(string warning)
        {
            InternalLogMessage(warning, MessageType.Warning);
        }
        void LogMessage(string message)
        {
            InternalLogMessage(message, MessageType.Message);
        }
        void LogError(string error)
        {
            LogMessageToFile(error, MessageType.Error);
            if (LogMessageToWindow(error, MessageType.Error))
            {
                return;
            }
            MessageBox.Show(error);
        }
        string FormatMessage(string message, MessageType messageType)
        {
            return string.Format("[{0:HH:MM:ss}][{1}]: {2}", DateTime.Now, MessageTypeToString(messageType), message);
        }
        bool LogMessageToWindow(string message, MessageType messageType)
        {
            if (_logWindowControl != null)
            {
                string msg = FormatMessage(message, messageType) + System.Environment.NewLine;
                _logWindowControl.LogRawMessage(msg);
                return true;
            }
            return false;
        }
        void LogMessageToFile(string message, MessageType messageType)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(_logFileName);
            try
            {
                sw.WriteLine(FormatMessage(message, messageType));
            }
            finally
            {
                sw.Close();
            }
        }
        string _logFileName;
    
        void CreateLogFile()
        {
            try
            {
                 _logFileName = Path.Combine(_workingDirectory, "PyAddIn.Log");
                 LogMessage("Log File Created");
            }
            catch (Exception e)
            {
                LogError("Error CreateLogFile:" + e.Message);
            }

        }

       

        private void InitDebuggerWindow()
        {
            try
            {

  
                var pypad = new UcPyPad();
                pypad.ActiveWindow.SetVariable("Controller", this);
                pypad.ActiveWindow.SetVariable("Adapter", _adapter);
                _debuggerHostWindow = _adapter.CreateHostWindow(pypad, "IronPython Interactive", "E6AD7E34-34CA-47E7-95EE-7C05D4128580");
            
            }
            catch (Exception e)
            {
                LogError("Error InitDebuggerWindow: " + e.Message);
            }
        }

        private void InitLogWindow()
        {
            try
            {
                CreateLogWindow();
                string assemblyName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                LogMessage(string.Format("Controller Loaded:{0} - {1}", _workingDirectory, assemblyName));
              
            }
            catch (Exception e)
            {
                LogError("Error InitLogWindoww: " + e.Message);
            }
        }

        ScriptMonitor _monitor;

        void LoadScriptEngine()
        {
            try
            {
                
                if (_monitor == null)
                {
                    var scriptDirectory = Path.Combine(_workingDirectory, "Scripts");
                    var scriptFileName = Path.Combine(scriptDirectory, "SSMSAddin.py");
                    _monitor = new ScriptMonitor(scriptFileName);

                    _monitor.OnScriptLoading += new ScriptMonitorEvent(_monitor_OnScriptLoading);
                    _monitor.OnScriptLoaded += new ScriptMonitorEvent(_monitor_OnScriptLoaded);
                    _monitor.OnScriptUnLoading += new ScriptMonitorEvent(_monitor_OnScriptUnLoading);
                    _monitor.OnEngineMessage += new MessageEvent(_monitor_OnEngineMessage);
                    _monitor.Activate();

                }
            }
            catch (Exception x)
            {
                LogError("Error LoadScriptEngine: " + x.Message);
            }     
        }

      

        void _monitor_OnEngineMessage(object sender, string Message)
        {
            LogMessage(Message);
        }

        void _monitor_OnScriptUnLoading(object sender, ScriptMonitorEventArgs e)
        {
            try
            {

                _handler.UnhookEvents(this);
            }
            catch (Exception x)
            {
                LogError("Error _monitor_OnScriptUnLoading: " + x.Message);
            }     
        }

        void _monitor_OnScriptLoading(object sender, ScriptMonitorEventArgs e)
        {
            

            try
            {

                e.Context.SetVariable("ObjectExplorer", _objectExplorer);
            }
            catch (Exception x)
            {
                LogError("Error _monitor_OnScriptLoading: " + x.Message);
            }     


        }
        dynamic _handler;
        void _monitor_OnScriptLoaded(object sender, ScriptMonitorEventArgs e)
        {
            try
            {
                
                LogMessage("Script loading...");
                _handler = e.Context.Scope.CreateDefaultConsumer();
                _handler.HookEvents(_adapter);
                LogMessage("Script Loaded");
            }
            catch (Exception x)
            {
                LogError("Error _monitor_OnScriptLoaded: " + x.Message);
            }     
        }

        object _objectExplorer = null;

        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {

                
                CreateLogFile();

                InitLogWindow();
                InitDebuggerWindow();

                LoadScriptEngine();

                
                try
                {
                    _handler.OnConnection(_adapter);
                }
                catch (Exception e)
                {
                    LogError("Error _handler.OnConnection: " + e.Message);
                }

                LogMessage("Controller Connected");
            }
            catch (Exception e)
            {
                LogError("Error AddInController.OnConnection: " + e.Message);
            }
        }

        void _adapter_OnAfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            try
            {

                _handler.CommandEvents_AfterExecute(Guid, ID, CustomIn, CustomOut);
            }
            catch (Exception e)
            {
                LogError("Error _handler.CommandEvents_AfterExecute: " + e.Message);
            }

        }

        void _adapter_OnBeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            try
            {
                _handler.CommandEvents_BeforeExecute(Guid, ID, CustomIn, CustomOut, ref CancelDefault);
            }
            catch (Exception e)
            {
                LogError("Error _handler.CommandEvents_BeforeExecute: " + e.Message);
            }

        }


        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            try
            {
                _handler.OnDisconnection(disconnectMode, ref custom);
            }
            catch (Exception e)
            {
                LogError("Error _handler.OnDisconnection: " + e.Message);
            }

        }
        
        public void OnAddInsUpdate(ref Array custom)
        {
            try
            {
                _handler.OnAddInsUpdate(custom);
            }
            catch (Exception e)
            {
                LogError("Error _handler.OnAddInsUpdate: " + e.Message);
            }

        }
        
        public void OnStartupComplete(ref Array custom)
        {
            try
            {
                _logWindow.Window.Visible = true;
                _debuggerHostWindow.Window.Visible = true;
                _handler.OnStartupComplete(custom);
            }
            catch (Exception e)
            {
                LogError("Error _handler.OnStartupComplete: " + e.Message);
            }

        }
  
        public void OnBeginShutdown(ref Array custom)
        {
            try
            {
                _handler.OnBeginShutdown(custom);
            }
            catch (Exception e)
            {
                LogError("Error _handler.OnBeginShutdown: " + e.Message);
            }

        }

        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            try
            {
                _handler.Exec(CmdName, ExecuteOption, VariantIn, VariantOut, Handled);
            }
            catch (Exception e)
            {
                LogError("Error _handler.Exec: " + e.Message);
            }

        }

        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
        {

            if (NeededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                try
                {
                    dynamic results = _handler.QueryStatus(CmdName, NeededText, StatusOption, CommandText);
                    StatusOption = results[0];
                    CommandText = results[1];
                    return;
                }
                catch (Exception e)
                {
                    LogError("Error _handler.QueryStatus: " + e.Message);
                }
            }

        }

       
    }
   


   
}
