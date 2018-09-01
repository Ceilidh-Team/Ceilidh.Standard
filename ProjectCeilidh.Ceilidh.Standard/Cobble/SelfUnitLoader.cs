using System.Linq;
using System.Reflection;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Cobble
{
    /// <summary>
    /// Load all execution units from Ceilidh.Standard
    /// </summary>
    /// <inheritdoc />
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
            // Resolve duplicates: prefer overrides from outside Ceilidh.Standard
            context.DuplicateResolver =
                       (pattern, implementations) =>
                       {
                           var impl = implementations.SingleOrDefault(x => x.GetType().Assembly != typeof(IUnitLoader).Assembly);

                           if (impl == null)
                               throw new AmbiguousDependencyException(pattern);

                           return impl;
                       };

            context.AddUnmanaged(_startOptions); // Add start options to the main graph

            // Add everything that has a CobbleExportAttribute
            foreach (var exp in typeof(IUnitLoader).Assembly.GetExportedTypes()
                .Where(x => x.GetCustomAttribute<CobbleExportAttribute>() != null && !_config.ExcludeClass.Contains(x.FullName)))
                context.AddManaged(exp);
        }
    }
}
