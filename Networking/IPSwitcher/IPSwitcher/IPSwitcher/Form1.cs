using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ACSR.Core.Processes;

namespace IPSwitcher
{
    public partial class Form1 : Form
    {
        BindingList<IPConfiguration> _configuration;
        public Form1()
        {
            InitializeComponent();
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            _configuration = new BindingList<IPConfiguration>();

            _configuration.Clear();
            foreach (var s in Properties.Settings.Default.ConfigurationStrings)
            {
                var cmd = new CmdLineHelper();
                cmd.ParseString(s);
                var config = new IPConfiguration();
                config.Configure(cmd);
                _configuration.Add(config);
            }
            UpdateContextMenu();
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            dataGridView1.DataSource = _configuration;

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void switchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var f = new FormAddEditEntry();
            f.Text = "Add";
             if (f.ShowDialog() == DialogResult.OK)
            {
                _configuration.Add(f.ucConfigEntry1.GetConfigurtaion());
            }
             UpdateConfiguration();
        }
        void UpdateContextMenu()
        {
            toolStripComboBoxConfigs.Items.Clear();
            foreach (var config in _configuration)
            {
                toolStripComboBoxConfigs.Items.Add(config.Name);
            }
            
        }
        void UpdateConfiguration()
        {
            Properties.Settings.Default.ConfigurationStrings.Clear();
            foreach (var entry in _configuration)
            {
                Properties.Settings.Default.ConfigurationStrings.Add(entry.ToString());
            }

            Properties.Settings.Default.Save();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var f = new FormAddEditEntry();
            f.Text = "Edit";
            var config = (IPConfiguration)dataGridView1.SelectedRows[0].DataBoundItem;
            f.ucConfigEntry1.SetConfiguration(config);
            if (f.ShowDialog() == DialogResult.OK)
            {
                var i = _configuration.IndexOf(config);
                _configuration.RemoveAt(i);
                _configuration.Insert(i, f.ucConfigEntry1.GetConfigurtaion());
            }
            UpdateConfiguration();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var config = (IPConfiguration)dataGridView1.SelectedRows[0].DataBoundItem;
            _configuration.Remove(config);
            UpdateConfiguration();
        }

        private void toolStripComboBoxConfigs_DropDownClosed(object sender, EventArgs e)
        {
            foreach (var config in _configuration)
            {
                if (config.Name.CompareTo(toolStripComboBoxConfigs.SelectedText) == 0)
                {
                    config.UpdateAdapter();
                }

            }
       }

    }
}
