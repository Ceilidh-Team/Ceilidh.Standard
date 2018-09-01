using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Config
{
    /// <summary>
    /// Loads configuration into the main <see cref="CobbleContext"/>
    /// </summary>
    public class ConfigUnitLoader : IUnitLoader
    {
        private static readonly string DefaultHomePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ceilidh");

        public string HomePath { get; }

        public CeilidhConfig Config { get; }

        public ConfigUnitLoader(CeilidhStartOptions options)
        {
            HomePath = options.UserDataPath ?? DefaultHomePath;
            Directory.CreateDirectory(HomePath);

            var configPath = options.ConfigFile ?? Path.Combine(HomePath, "config.xml");

            if (File.Exists(configPath))
                using (var configFile = File.OpenRead(configPath))
                {
                    var serializer = new XmlSerializer(typeof(CeilidhConfig));
                    Config = (CeilidhConfig)serializer.Deserialize(configFile);
                }
            else
                Config = CeilidhConfig.DefaultConfig;

            Config.HomePath = HomePath;

            if (Config.Culture != null)
                CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = Config.Culture;

            // TODO: A config controller is necessary to allow extension and saving
        }

        public void RegisterUnits(CobbleContext context)
        {
            context.AddUnmanaged(Config);
        }
    }
}
