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
    public partial class UcBlankControl : UserControl
    {

        
        public UcBlankControl()
        {
            InitializeComponent();
        }

        public void AddControl(Control Control)
        {
            Controls.Add(Control);
        }
       
        private void UcBlankControl_Load(object sender, EventArgs e)
        {
            
        }
    }
}
