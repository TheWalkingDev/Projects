using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ACSR.Controls.ThirdParty.Python;

namespace PyPad
{
    public partial class Form1 : Form
    {
        UcPyPad _pypad;
        public Form1()
        {
            InitializeComponent();
            Text = "PyPad";
            _pypad = new UcPyPad();
            _pypad.SetVariable("HostForm", this);
            _pypad.Dock = DockStyle.Fill;
            Controls.Add(_pypad);
            Width = 800;
            Height = 600;
            _pypad.RunScript("");
        }
    }
}
