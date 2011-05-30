using System;
using EnvDTE;
using System.Windows.Forms;
namespace PyAddIn.Controller
{
    public interface IAddInAdapter
    {
        event AddInMessageEvent OnMessage;
        event AddInMessageEvent OnWarning;
        event AddInMessageEvent OnError;
        event _dispCommandEvents_BeforeExecuteEventHandler OnBeforeExecute;
        event _dispCommandEvents_AfterExecuteEventHandler OnAfterExecute;

        string CommonUIAssemblyLocation { get; set; }
        IToolWindowContext CreateToolWindow(string typeName, string assemblyLocation, string Caption, Guid uiTypeGuid);
        IToolWindowContext CreateCommonUIControl(string typeName, string caption, Guid guid);
        IToolWindowContext CreateHostWindow(Control controlToHost, string caption, string guid);
        void LockAround(object lockObject, Action<object> callback, object arg);
    }
}
