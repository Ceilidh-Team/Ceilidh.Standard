using System;
using System.IO;
using System.Xml.Serialization;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Config
{
    public class ConfigUnitLoader : IUnitLoader
    {
        private static readonly string DefaultHomePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ceilidh");
        private static readonly string DefaultConfigPath = Path.Combine(DefaultHomePath, "config.xml");

        public string HomePath { get; }

        public CeilidhConfig Config { get; }

        public ConfigUnitLoader(CeilidhStartOptions options)
        {
            HomePath = DefaultHomePath;
            Directory.CreateDirectory(HomePath);

            if (File.Exists(DefaultConfigPath))
                using (var configFile = File.OpenRead(DefaultConfigPath))
                {
                    var serializer = new XmlSerializer(typeof(CeilidhConfig));
                    Config = (CeilidhConfig)serializer.Deserialize(configFile);
                }
            else
                Config = CeilidhConfig.DefaultConfig;
        }

        public void RegisterUnits(CobbleContext context)
        {
            context.AddUnmanaged(Config);
        }
    }
}
