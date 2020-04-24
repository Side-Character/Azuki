using Discore;
using System;
using System.Collections.Generic;

namespace Chris {
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("Config")]
    public class Config {
        [System.Xml.Serialization.XmlElementAttribute("Token")]
        public string Token { get; set; }
        [System.Xml.Serialization.XmlElementAttribute("Defaultstartchar")]
        public string DefaultStartAsString { get { return DefaultStartChar.ToString(); } set { DefaultStartChar = value[0]; } }
        [System.Xml.Serialization.XmlElementAttribute("AdminId")]
        public List<ulong> AdminIds = new List<ulong>();
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
