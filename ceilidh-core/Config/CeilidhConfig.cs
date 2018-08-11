using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Ceilidh.Core.Config
{
    [XmlRoot("CeilidhConfig", IsNullable = false)]
    public class CeilidhConfig
    {
        public static readonly CeilidhConfig DefaultConfig = new CeilidhConfig
        {
            Culture = null,
            ExcludeClass = new List<string>(),
            Plugins = new List<string>()
        };

        private static readonly XmlSerializer ConfigSerializer = new XmlSerializer(typeof(CeilidhConfig));

        [XmlElement(ElementName = "culture", IsNullable = false)]
        public string Culture;

        [XmlArray(ElementName = "exclusions")] [XmlArrayItem("exclude")]
        public List<string> ExcludeClass;

        [XmlArray(ElementName = "plugins")] [XmlArrayItem("plugin")]
        public List<string> Plugins;

        public void WriteConfig(Stream str)
        {
            using (var xml = XmlWriter.Create(str, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            }))
                ConfigSerializer.Serialize(xml, this, new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty}));
        }

        public static CeilidhConfig ReadConfig(Stream str)
        {
            try
            {
                return (CeilidhConfig) ConfigSerializer.Deserialize(str);
            }
            catch
            {
                return DefaultConfig;
            }
        }
    }
}