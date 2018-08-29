using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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
        public static readonly OSPlatform BuildPlatform =
#if WIN32
            OSPlatform.Windows;
#elif OSX
            OSPlatform.OSX;
#else
            OSPlatform.Linux;
#endif

        public static void Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(BuildPlatform))
                throw new PlatformNotSupportedException($@"This binary was built for ""{BuildPlatform}"", which is incompatible with the current platform.");

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

                var impl = loader.Execute(config.ExcludeClass);
                if (!impl.TryGetSingleton<ILocalizationController>(out var loc))
                    throw new Exception("Cannot load the localization controller: this is required.");

                LocalizedException.LocalizationController = loc;
                Console.WriteLine(loc.Translate("Hello", "Ceilidh"));

                Console.ReadLine();
            });
        }
    }
}