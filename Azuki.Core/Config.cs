using Azuki.Core.Properties;
using Discore;
using System;
using System.Collections.Generic;

namespace Azuki.Core {
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Config")]
    public class AzukiConfig {
        [System.Xml.Serialization.XmlElementAttribute("Token")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Pending>")]
        public string Token { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("Defaultstartchar")]
        public string DefaultStartAsString { get { return DefaultStartChar.ToString(Resources.Culture); } set { if (value is null) { throw new ArgumentNullException(nameof(value)); }; DefaultStartChar = value[0]; } }
        [System.Xml.Serialization.XmlElementAttribute("AdminId")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for Deserialization.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Pending>")]
        public List<ulong> AdminIds { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public char DefaultStartChar { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public List<Snowflake> Admins {
            get {
                List<Snowflake> admins = new List<Snowflake>();
                foreach (ulong id in AdminIds) {
                    admins.Add(new Snowflake(id));
                }
                return admins;
            }
        }
    }
}
