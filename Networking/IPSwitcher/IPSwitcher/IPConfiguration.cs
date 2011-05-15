using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.Core.Processes;
using System.Management;

namespace IPSwitcher
{
    public class IPConfiguration
    {
        string _IpAddress;

        public string IpAddress
        {
            get { return _IpAddress; }
            set { _IpAddress = value; }
        }
        string _Gateway;

        public string Gateway
        {
            get { return _Gateway; }
            set { _Gateway = value; }
        }
        string _mask;

        public string Mask
        {
            get { return _mask; }
            set { _mask = value; }
        }
        string _DNS;

        public string DNS
        {
            get { return _DNS; }
            set { _DNS = value; }
        }
        string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        string _adapterName;

        public string AdapterName
        {
            get { return _adapterName; }
            set { _adapterName = value; }
        }

        public void Configure(ICommandParameters commandlineParamters)
        {
            _IpAddress = commandlineParamters.ParamAfterSwitch("IpAddress");
            _mask = commandlineParamters.ParamAfterSwitch("Mask");
            _Gateway = commandlineParamters.ParamAfterSwitch("Gateway");
            _DNS = commandlineParamters.ParamAfterSwitch("DNS");
            _Name = commandlineParamters.ParamAfterSwitch("Name");
            _adapterName = commandlineParamters.ParamAfterSwitch("AdapterName");

        }
        public override string ToString()
        {
            return string.Format("-Name {0} -AdapterName {1} -IpAddress {2} -Mask {3} -Gateway {4} -DNS {5}",
                _Name,
                _adapterName,
                _IpAddress,
                _mask,
                _Gateway,
                _DNS);
        }
        public IEnumerable<dynamic> GetAdapters()
        {
            ManagementClass objMC = new ManagementClass(
                "Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach (dynamic objMO in objMOC)
            {

                //new PyInspector().SetVariable("objMC", objMC).SetVariable("objMO", objMO).Inspect();
                if (objMO["IPEnabled"])
                {
                    yield return new Adapter(objMO,
                        objMO["Caption"].ToString(),
                        objMO["ServiceName"].ToString());
                }
            }
        }

     
        public IPConfiguration()
        {


           // new PyInspector().SetVariable("objMC", objMC).Inspect();

        }

        internal void UpdateAdapter()
        {
            foreach (var adapter in GetAdapters())
            {
                if (adapter.Name.CompareTo(_adapterName) == 0)
                {

                    ManagementBaseObject objNewIP = null;
                    ManagementBaseObject objSetIP = null;
                    ManagementBaseObject objNewGate = null;


                    objNewIP = adapter.ManagementObject.GetMethodParameters("EnableStatic");
                    objNewGate = adapter.ManagementObject.GetMethodParameters("SetGateways");



                    //Set DefaultGateway

                    objNewGate["DefaultIPGateway"] = new string[] { _Gateway };
                    objNewGate["GatewayCostMetric"] = new int[] { 1 };


                    //Set IPAddress and Subnet Mask

                    objNewIP["IPAddress"] = new string[] { _IpAddress };
                    objNewIP["SubnetMask"] = new string[] { _mask };

                    objSetIP = adapter.ManagementObject.InvokeMethod("EnableStatic", objNewIP, null);
                    objSetIP = adapter.ManagementObject.InvokeMethod("SetGateways", objNewGate, null);
                      
                        
                    
                    
                }
            }
        }
    }

    public class Adapter
    {
        string _caption;

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }
        string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        string _description;
        private dynamic _managementObject;

        public dynamic ManagementObject
        {
            get { return _managementObject; }
            set { _managementObject = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }
        public Adapter(dynamic managementObject, string caption, string name)
        {
            _name = name;
            _caption = caption;
            this._managementObject = managementObject;
        }

        
    }
}
