using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ACSR.SqlServer.Addin.Core.UI
{
    [ComVisible(true)]
    public partial class UcLogWindow : UserControl
    {
        public void LogMessage(string message)
        {
            LogRawMessage(string.Format("[{0:HH:MM:ss}]: {1}{2}", DateTime.Now, message, System.Environment.NewLine));
        }
        public void LogRawMessage(string message)
        {
            richTextBox1.AppendText(message);
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        public UcLogWindow()
        {
            InitializeComponent();
            LogMessage(string.Format("Log Window Loaded .NET {0} - {1} ", Environment.Version, System.Reflection.Assembly.GetExecutingAssembly().Location));
        }
    }
}
