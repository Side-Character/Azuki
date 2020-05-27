using Azuki.Core.Properties;
using Discore;
using System;
using System.Collections.Generic;

namespace Azuki.Core.Config {
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Config")]
    public class Config {
        [System.Xml.Serialization.XmlElementAttribute("Token")]
        public string Token { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("Defaultstartchar")]
        public string DefaultStartAsString { get { return DefaultStartChar.ToString(Resources.Culture); } set { if (value is null) { throw new ArgumentNullException(nameof(value)); }; DefaultStartChar = value[0]; } }
        [System.Xml.Serialization.XmlElementAttribute("AdminId")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Needed for Deserialization.")]
        public List<ulong> AdminIds { get; set;}
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
