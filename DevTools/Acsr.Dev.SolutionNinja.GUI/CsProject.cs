using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ACSR.Core.Strings;

namespace Acsr.Dev.SolutionNinja.GUI
{


    public class CsProject
    {
        XmlDocument _doc;
        string _fileName;
        public CsProject(string FileName)
        {
            _fileName = FileName;

        }


        private void LoadDLLReferences(XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                Console.WriteLine(node.Attributes["Include"].Value);
            }

        }
        public void Load()
        {
            _doc = new XmlDocument();
            _doc.LoadXml(StringTools.FileToString(_fileName));
            XmlNamespaceManager nm = new XmlNamespaceManager(_doc.NameTable);

            nm.AddNamespace("n", _doc.DocumentElement.Attributes["xmlns"].Value);
            nm.AddNamespace(string.Empty, _doc.DocumentElement.Attributes["xmlns"].Value);

            //var nodeDll = _doc.DocumentElement.SelectNodes("/n:Project/n:ItemGroup/n:Reference", nm);

            var refs =new CsReferences(_doc, nm);
            foreach (var dllRef in refs.GetDLLReferences())
            {
                Console.WriteLine(dllRef.Include);
                if (dllRef.SpecificVersion) 

                    dllRef.SpecificVersion = false;
              if (dllRef.HintPath != null)
                Console.WriteLine(dllRef.HintPath);

                var attribs = dllRef.GetIncludeAttribs();
                string ver;
                if (attribs.TryGetValue("Version", out ver))
                {
                    attribs["Version"] = "6.0.0.0";
                }
                dllRef.SetIncludeAttribs(attribs);
            }
           // LoadDLLReferences(nodeDll);

        }
        public void Save()
        {
            _doc.Save(_fileName);
        }
    }

    public class CsReferences
    {
        XmlNode _projectNode;
        //XmlNode _itemGroup;
        //XmlNodeList _referenceNodes;
        XmlNamespaceManager nm;
        public CsReferences(XmlNode Anode, XmlNamespaceManager nm)
        {
            this.nm = nm;
            _projectNode =  Anode.SelectSingleNode("/n:Project", nm);
            //_itemGroup = Anode.SelectSingleNode("/n:Project/n:ItemGroup", nm);
        }

        public IEnumerable<CsDLLReference> GetDLLReferences()
        {
            var nl = _projectNode.SelectNodes("./n:ItemGroup/n:Reference", nm);
            foreach (XmlNode n in nl)
            {
                yield return new CsDLLReference(n, nm);

            }
        }

    }

    public class CsDLLReference
    {
        XmlNode _node;
        XmlNode _specificVersionNode;
        XmlNode _hintPathNode;
        XmlNamespaceManager nm;
        public CsDLLReference(XmlNode node, XmlNamespaceManager nm)
        {
            _node = node;
            this.nm = nm;
        }
        
        public Dictionary<string, string> GetIncludeAttribs()
        {
            var res = new Dictionary<string, string>();
            foreach (var s in this.Include.Split(','))
            {
                var values = s.Split('=');
                var name = values[0].Trim();
                if (values.Length > 1)
                {
                    res[name] = values[1];
                }
                else
                {
                    res[name] = null;
                }
            }
            return res;
        }
        public void SetIncludeAttribs(Dictionary<string, string> Attribs)
        {
            var sb = new StringBuilder();
            foreach (var attrib in Attribs)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                if (attrib.Value != null)
                {
                    sb.Append(string.Format("{0}={1}", attrib.Key, attrib.Value));
                }
                else
                {
                    sb.Append(string.Format("{0}", attrib.Key));
                }
            }
            Include = sb.ToString();

        }

        public string Include
        {
            get
            {               
                return _node.Attributes["Include"].Value;
            }
            set
            {
                _node.Attributes["Include"].Value = value;
            }
        }
        public bool SpecificVersion
        {
            get
            {
                if (_specificVersionNode == null)
                    _specificVersionNode = _node.SelectSingleNode("./n:SpecificVersion", nm);
                if (_specificVersionNode == null)
                {
                        return false;
                }
                else
                {
                    return bool.Parse(_specificVersionNode.InnerText);
                }
            }
            set
            {

                if (_specificVersionNode == null)
                {

                    var n = _node.OwnerDocument.CreateElement("SpecificVersion", nm.DefaultNamespace);
                    //n.Name = "SpecificVersion";
                    _node.AppendChild(n);
                    _specificVersionNode = n;
                }
                _specificVersionNode.InnerText = value.ToString();


            }
        }
        public string HintPath
        {
            get
            {
                if (_hintPathNode == null)
                    _hintPathNode = _node.SelectSingleNode("./n:HintPath", nm);
                if (_hintPathNode == null)
                {
                    return null;
                }
                else
                {
                    return _hintPathNode.InnerText;
                }
            }
            set
            {

                if (_hintPathNode == null)
                {

                    var n = _node.OwnerDocument.CreateElement("HintPath", nm.DefaultNamespace);
                    //n.Name = "SpecificVersion";
                    _node.AppendChild(n);
                    _hintPathNode = n;
                }
                _specificVersionNode.InnerText = value;


            }
        }

    }
    

}
