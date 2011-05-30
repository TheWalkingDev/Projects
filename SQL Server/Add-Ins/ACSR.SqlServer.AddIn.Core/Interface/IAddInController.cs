using System;
using PyAddIn.Controller;
namespace PyAddIn
{
    public  interface IAddInController
    {
        //IToolWindowContext CreateToolWindow(string typeName, string assemblyLocation, string Caption, Guid uiTypeGuid, EnvDTE80.DTE2 applicationObject, EnvDTE.AddIn addinInstance);
        void Exec(string CmdName, EnvDTE.vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled);
        void OnAddInsUpdate(ref Array custom);
        void OnBeginShutdown(ref Array custom);
        void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref Array custom);
        void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref Array custom);
        void OnStartupComplete(ref Array custom);
        void QueryStatus(string CmdName, EnvDTE.vsCommandStatusTextWanted NeededText, ref EnvDTE.vsCommandStatus StatusOption, ref object CommandText);
    }
}
