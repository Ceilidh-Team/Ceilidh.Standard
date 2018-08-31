using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProjectCeilidh.Ceilidh.Standard.Config
{
    [XmlRoot(ElementName = "CeilidhConfig", IsNullable = false)]
    public class CeilidhConfig
    {
        public static readonly CeilidhConfig DefaultConfig = new CeilidhConfig
        {
            Culture = null,
            ExcludeClass = new HashSet<string>(),
            Plugins = new List<string>()
        };

        [XmlIgnore]
        public string HomePath;

        [XmlElement(ElementName = "culture", IsNullable = false)]
        public string Culture;

        [XmlArray(ElementName = "exclusions")]
        [XmlArrayItem("exclude")]
        public HashSet<string> ExcludeClass;

        [XmlArray(ElementName = "plugins")]
        [XmlArrayItem("plugin")]
        public List<string> Plugins;
    }
}
