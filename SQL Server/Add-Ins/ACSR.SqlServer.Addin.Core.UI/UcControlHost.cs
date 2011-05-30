using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ACSR.SqlServer.Addin.Core.UI
{
    [ComVisible(true)]
    public partial class UcControlHost : UserControl
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);  

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(
              IntPtr hObject   // handle to object
              );

        private const UInt32 WM_MOVE = 0x0003;
        private const UInt32 WM_SIZE = 0x0005;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("User32.dll")]

        private static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        private IntPtr MakeParam(int LoWord, int HiWord)
        {
            int i = (HiWord << 16) | (LoWord & 0xffff);
            return new IntPtr(i);
        }
        public object LoadControlFromAssembly(string dllName, string typeName)
        {
            Assembly assembly = Assembly.LoadFrom(dllName);
            var type = assembly.GetType(typeName);
            object ClassObj = Activator.CreateInstance(type);
            Controls.Add((Control)ClassObj);
            return ClassObj;
        }
        public UcControlHost()
        {
            InitializeComponent();
            Resize += new EventHandler(UcControlHost_Resize);
            _hostedControlHandle = IntPtr.Zero;
        }
        IntPtr _hostedControlHandle;

        [DispId(-515)]
        public void HostChildControl(IControlContext controlContext)
        {
            if (_hostedControlHandle != IntPtr.Zero)
            {
                CloseHandle(_hostedControlHandle);
            }
            _hostedControlHandle = controlContext.Handle;
            SetParent(_hostedControlHandle, this.Handle);
            MoveWindow(_hostedControlHandle, 0, 0, Width, Height, true);
        }

        void UcControlHost_Resize(object sender, EventArgs e)
        {
            MoveWindow(_hostedControlHandle, 0, 0, Width, Height, true);
        }
    }

    public struct COPYDATASTRUCT
    {
	    public IntPtr dwData;
	    public int cbData;
	    [MarshalAs(UnmanagedType.LPStr)]
	    public string lpData;
	 }

    [ComVisible(true)]
    public class ControlContext : ACSR.SqlServer.Addin.Core.UI.IControlContext
    {
        Control _control;
        IntPtr _handle;

        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }
        public ControlContext(Control control)
        {
            _control = control;
            _handle = _control.Handle;
        }
    }
}
