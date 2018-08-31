using System;
using System.Linq;
using System.Reflection;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Cobble
{
    public class SelfUnitLoader : IUnitLoader
    {
        private readonly CeilidhStartOptions _startOptions;
        private readonly CeilidhConfig _config;

        public SelfUnitLoader(ConfigUnitLoader configUnit, CeilidhStartOptions startOptions)
        {
            _config = configUnit.Config;
            _startOptions = startOptions;
        }

        public void RegisterUnits(CobbleContext context)
        {
            context.DuplicateResolver =
                       (pattern, implementations) =>
                       {
                           var impl = implementations.SingleOrDefault(x => x.GetType().Assembly != typeof(IUnitLoader).Assembly);

                           if (impl == null)
                               throw new AmbiguousDependencyException(pattern);

                           return impl;
                       };

            context.AddUnmanaged(_startOptions);

            foreach (var exp in typeof(IUnitLoader).Assembly.GetExportedTypes()
                .Where(x => x.GetCustomAttribute<CobbleExportAttribute>() != null && !_config.ExcludeClass.Contains(x.FullName)))
                context.AddManaged(exp);
        }
    }
}
