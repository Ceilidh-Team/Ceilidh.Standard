using System.IO;
using System.Linq;
using System.Reflection;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Plugin
{
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
            foreach (var file in dir.EnumerateFileSystemInfos())
            {
                var asm = Assembly.LoadFrom(file.FullName);

                var pluginContext = new CobbleContext();

                foreach (var unit in asm.GetExportedTypes().Where(x => x != typeof(IUnitLoader) && typeof(IUnitLoader).IsAssignableFrom(x)))
                    pluginContext.AddManaged(unit);

                pluginContext.Execute();

                if (pluginContext.TryGetImplementations<IUnitLoader>(out var loaders))
                    foreach (var loader in loaders)
                        loader.RegisterUnits(context);
            }
        }
    }
}
