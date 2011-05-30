using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using EnvDTE;
using ACSR.SqlServer.AddIn.Core.Interface;
using System.Windows.Forms;
using ACSR.SqlServer.Addin.Core.UI;

namespace PyAddIn.Controller
{
    public delegate void AddInMessageEvent (object sender, string message, MessageType messageType);
    public class AddInAdapter : IAddInAdapter
    {
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        public DTE2 ApplicationObject
        {
            get
            {
                return _applicationObject;
            }
        }
        public AddIn AddInInstance
        {
            get
            {
                return _addInInstance;
            }
        }
        public CommandEvents CommandEvents
        {
            get
            {
                return _CommandEvents;
            }
        }

        public event AddInMessageEvent OnMessage;
        public event AddInMessageEvent OnWarning;
        public event AddInMessageEvent OnError;
        public event _dispCommandEvents_BeforeExecuteEventHandler OnBeforeExecute;
        public event _dispCommandEvents_AfterExecuteEventHandler OnAfterExecute;
       // public event 
        CommandEvents _CommandEvents;

        public AddInAdapter(object addInInst)
        {
            _addInInstance = (AddIn)addInInst;          
            _applicationObject = (DTE2)_addInInstance.DTE;

            _CommandEvents = _applicationObject.Events.get_CommandEvents("{52692960-56BC-4989-B5D3-94C47A513E8D}", 1);
            _CommandEvents.AfterExecute += new _dispCommandEvents_AfterExecuteEventHandler(_CommandEvents_AfterExecute);
            _CommandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(_CommandEvents_BeforeExecute);
        }

        void _CommandEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            if (OnBeforeExecute != null)
            {
                OnBeforeExecute(Guid, ID, CustomIn, CustomOut, ref CancelDefault);
            }
        }

        void _CommandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            if (OnAfterExecute != null)
            {
                OnAfterExecute(Guid, ID, CustomIn, CustomOut);
            }
        }


        public void LogMessage(string message)
        {
            if (OnMessage != null)
            {
                OnMessage(this, message, MessageType.Message);
            }
                
        }
        public void LogWarning(string warning)
        {
            if (OnWarning != null)
            {
                OnWarning(this, warning, MessageType.Message);
            }

        }

        public void LogError(string error)
        {
            if (OnError != null)
            {
                OnError(this, error, MessageType.Message);
            }
        }

        public string CommonUIAssemblyLocation { get; set; }

        public IToolWindowContext CreateCommonUIControl(string typeName, string caption, Guid guid)
        {
            return CreateToolWindow(CommonUIAssemblyLocation, typeName, caption, guid);
        }

        public void HostWindow(dynamic hostWindow, Control controlToHost)
        {
            hostWindow.HostChildControl(new ControlContext(controlToHost));
        }

        public IToolWindowContext CreateHostWindow(Control controlToHost, string caption, string guid)
        {
            controlToHost.Dock = System.Windows.Forms.DockStyle.Fill;
            controlToHost.Visible = true;
            var window = CreateCommonUIControl("ACSR.SqlServer.Addin.Core.UI.UcControlHost", caption, new Guid(guid));
            HostWindow(((dynamic)window.ControlObject), controlToHost);
            return window;
        }

        public IToolWindowContext CreateToolWindow(string assemblyLocation, string typeName, string Caption, Guid uiTypeGuid)
        {
            Windows2 win2 = _applicationObject.Windows as Windows2;
            if (win2 != null)
            {
                object controlObject = null;

                Window toolWindow = win2.CreateToolWindow2(_addInInstance, assemblyLocation, typeName, Caption, "{" + uiTypeGuid.ToString() + "}", ref controlObject);
                Window2 toolWindow2 = (Window2)toolWindow;
                toolWindow.Linkable = false;
                //toolWindow.IsFloating = true;
                try
                {
                    toolWindow.WindowState = vsWindowState.vsWindowStateMaximize;
                }
                catch
                {
                }
                
                toolWindow.Visible = true;
                
                return new ToolWindowContext(toolWindow, toolWindow2, controlObject);
            }
            return null;
        }

        public void LockAround(object lockObject, Action<object> callback, object arg)
        {
            lock (lockObject)
            {
                callback(arg);
            }
        }

        public dynamic AddNamedCommand(string name, string buttonText, string toolTip, int ImageId)
        {
            try
            {
                
                object[] contextGUIDS = new object[] { };
                int commandStatus = (int)vsCommandStatus.vsCommandStatusSupported +
                    (int)vsCommandStatus.vsCommandStatusEnabled;
                int commandStyle = (int)vsCommandStyle.vsCommandStylePictAndText;

                dynamic commands = _addInInstance.DTE.Commands;
                dynamic cmdResult = commands.AddNamedCommand2(_addInInstance, name,
                buttonText, toolTip, true, ImageId,
                ref contextGUIDS,
                commandStatus,
                commandStyle,
                vsCommandControlType.vsCommandControlTypeButton);
                return cmdResult;
            }
            catch (Exception x)
            {
                LogError("AddNamedCommand() Error: " + x.Message);
                return null;
            }
        }
    }

    public class ToolWindowContext : PyAddIn.Controller.IToolWindowContext
    {
        public Window Window { get; set; }
        public Window Window2 { get; set; }
        public dynamic ControlObject { get; set; }
        public ToolWindowContext(Window Window, Window2 Window2, dynamic ControlObject)
        {
            this.Window = Window;
            this.Window2 = Window2;
            this.ControlObject = ControlObject;
        }
    }
}
