using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IPSwitcher
{
    public partial class UcConfigEntry : UserControl
    {
        public UcConfigEntry()
        {
            InitializeComponent();
            comboBox.DisplayMember = "Caption";
            foreach (var adapter in new IPConfiguration().GetAdapters())
            {
                comboBox.Items.Add(adapter);
            }
        }

        private void UcConfigEntry_Load(object sender, EventArgs e)
        {
            
        }

        public void SetConfiguration(IPConfiguration configuration)
        {
            textBoxName.Text = configuration.Name;
            textBoxIP.Text = configuration.IpAddress;
            textBoxMask.Text = configuration.Mask;
            textBoxGateway.Text = configuration.Gateway;
            textBoxDNS.Text = configuration.DNS;
            
            foreach (var item in comboBox.Items)
            {
                var adapter = (Adapter)item;
                if (adapter.Name.CompareTo(configuration.AdapterName) == 0)
                {
                    comboBox.SelectedItem = adapter;
                }
            }
        }
        public IPConfiguration GetConfigurtaion()
        {
            var adapter = (Adapter)comboBox.SelectedItem;
            var configuration = new IPConfiguration();
            configuration.Name = textBoxName.Text;
            configuration.AdapterName = adapter.Name;
            configuration.IpAddress = textBoxIP.Text;
            configuration.Mask = textBoxMask.Text;
            configuration.Gateway = textBoxGateway.Text;
            configuration.DNS = textBoxDNS.Text;
            return configuration;
        }
    }
}
