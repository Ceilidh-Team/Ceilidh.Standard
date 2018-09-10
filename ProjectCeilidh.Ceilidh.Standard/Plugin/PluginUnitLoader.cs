using System.IO;
using System.Linq;
using System.Reflection;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Plugin
{
    /// <summary>
    /// Loads plugin assemblies into the main context
    /// </summary>
    public class PluginUnitLoader : IUnitLoader
    {
        private readonly ConfigUnitLoader _configUnitLoader;

        public PluginUnitLoader(ConfigUnitLoader config)
        {
            _configUnitLoader = config;
        }

        public void RegisterUnits(CobbleContext context)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_configUnitLoader.HomePath, "plugins"));
            foreach (var file in dir.EnumerateFileSystemInfos().Select(x => x.FullName).Concat(_configUnitLoader.Config.Plugins))
            {
                var asm = Assembly.LoadFrom(file);

                var pluginContext = new CobbleContext();

                pluginContext.AddUnmanaged(_configUnitLoader.Config);

                foreach (var unit in asm.GetExportedTypes().Where(x => x != typeof(IUnitLoader) && typeof(IUnitLoader).IsAssignableFrom(x)))
                    pluginContext.AddManaged(unit);

                pluginContext.Execute();

                if (!pluginContext.TryGetImplementations<IUnitLoader>(out var loaders)) continue;

                foreach (var loader in loaders)
                    loader.RegisterUnits(context);
            }
        }
    }
}
