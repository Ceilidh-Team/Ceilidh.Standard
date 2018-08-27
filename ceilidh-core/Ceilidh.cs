using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Ceilidh.Core.Config;
using Ceilidh.Core.Plugin;
using Ceilidh.Core.Plugin.Archive;
using Ceilidh.Core.Util;
using Ceilidh.Core.Vendor.Contracts;
using CommandLine;

namespace Ceilidh.Core
{
    internal static class Ceilidh
    {
        public const PlatformID BUILD_PLATFORM =
#if WIN32
            PlatformID.Win32NT;
#else
            PlatformID.Unix;

#endif

        public static PluginImplementationMap Implementations { get; private set; }

        public static void Main(string[] args)
        {
#if WIN32
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
#else
            if (Environment.OSVersion.Platform != PlatformID.MacOSX && Environment.OSVersion.Platform != PlatformID.Unix)
#endif
                throw new PlatformNotSupportedException($@"This binary was built for ""{BUILD_PLATFORM}"", but the current platform is ""{Environment.OSVersion.Platform}.""");

            Parser.Default.ParseArguments<CeilidhArguments>(args).WithParsed(x =>
            {
                x.UserDataPath = x.UserDataPath ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ceilidh");
                x.ConfigFile = x.ConfigFile ?? Path.Join(x.UserDataPath, "config.xml");
                Directory.CreateDirectory(x.UserDataPath);

                var config = CeilidhConfig.DefaultConfig;
                if (File.Exists(x.ConfigFile))
                    using (var configStream = File.OpenRead(x.ConfigFile))
                        config = CeilidhConfig.ReadConfig(configStream);

                using (var configStream = File.Open(x.ConfigFile, FileMode.Create))
                    config.WriteConfig(configStream);

                if (config.Culture != null)
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture =
                        CultureInfo.GetCultureInfo(config.Culture);

                var arch = new PluginArchive(Path.Join(x.UserDataPath, "plugins"));

                var loader = new PluginLoader(arch, config);
                loader.QueueLoad(typeof(Ceilidh).Assembly);
                foreach (var pluginPath in config.Plugins)
                    loader.QueueLoad(pluginPath);
                foreach (var installedPlugin in arch.InstalledPlugins())
                    loader.QueueLoad(installedPlugin);

                Implementations = loader.Execute(config.ExcludeClass);
                if (Implementations.TryGetSingleton<ILocalizationController>(out var single))
                    Console.WriteLine(single.Translate("Hello", "Ceilidh"));

                Console.ReadLine();
            });
        }
    }
}