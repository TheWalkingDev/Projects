﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IPSwitcher.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>-Name Gaming -AdapterName RTL8167 -IpAddress 10.0.0.65 -Mask 255.255.255.0 -Gateway 10.0.0.25 -DNS 10.0.0.20</string>
  <string>-Name Browsing -AdapterName RTL8167 -IpAddress 10.0.0.65 -Mask 255.255.255.0 -Gateway 10.0.0.20 -DNS 10.0.0.20</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection ConfigurationStrings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["ConfigurationStrings"]));
            }
            set {
                this["ConfigurationStrings"] = value;
            }
        }
    }
}
