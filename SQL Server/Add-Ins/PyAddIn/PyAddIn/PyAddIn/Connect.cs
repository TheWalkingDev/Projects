using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using PyAddIn.Controller;
using System.Windows.Forms;
namespace PyAddIn
{
	/// <summary>The object for implementing an Add-in.</summary>
    public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        IAddInController _controller;
        IAddInAdapter _adapter;

        int instanceID = 1;
        void ShowMessage(string Message)
        {
            MessageBox.Show(string.Format("[{0}] {1}", instanceID, Message));
        }


		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
            try
            {
                _adapter = new AddInAdapter(addInInst);
                _controller = new AddInController(_adapter);
                _controller.OnConnection(application, connectMode, addInInst, ref custom);
            }
            catch (Exception e)
            {
                ShowMessage("Error Connect.OnConnection: " + e.Message);
            }
            
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
            _controller.OnDisconnection(disconnectMode, ref custom);
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		public void OnAddInsUpdate(ref Array custom)
		{
            _controller.OnAddInsUpdate(ref custom);
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		public void OnStartupComplete(ref Array custom)
		{
            _controller.OnStartupComplete(ref custom);
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		public void OnBeginShutdown(ref Array custom)
		{
            _controller.OnBeginShutdown(ref custom);
		}
        /// <summary/>
        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            _controller.Exec(CmdName, ExecuteOption, ref VariantIn, ref VariantOut, ref Handled);
        }
        /// <summary/>
        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
        {
            _controller.QueryStatus(CmdName, NeededText, ref StatusOption, ref CommandText);
        }
        
    }
}