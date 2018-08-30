using System.Linq;
using System.Reflection;
using ProjectCeilidh.Cobble;

namespace ProjectCeildih.Ceilidh.Standard.Cobble
{
    public class SelfUnitLoader : IUnitLoader
    {
        private readonly CeilidhStartOptions _startOptions;

        public SelfUnitLoader(CeilidhStartOptions startOptions)
        {
            _startOptions = startOptions;
        }

        public void RegisterUnits(CobbleContext context)
        {
            context.AddUnmanaged(_startOptions);

            foreach (var exp in typeof(IUnitLoader).Assembly.GetExportedTypes()
                .Where(x => x.GetCustomAttribute<CobbleExportAttribute>() != null))
                context.AddManaged(exp);
        }
    }
}
