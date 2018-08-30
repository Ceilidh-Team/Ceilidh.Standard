using System.Linq;
using ProjectCeilidh.Ceilidh.Standard;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.ConsoleShell
{
    public static class ConsoleShell
    {
        private static void Main(string[] args)
        {
            var loadContext = new CobbleContext();

            loadContext.AddUnmanaged(new CeilidhStartOptions(args));

            foreach (var unit in typeof(IUnitLoader).Assembly.GetExportedTypes()
                .Where(x => x != typeof(IUnitLoader) && typeof(IUnitLoader).IsAssignableFrom(x)))
                loadContext.AddManaged(unit);
            loadContext.Execute();
            if (!loadContext.TryGetImplementations<IUnitLoader>(out var impl)) return;

            var ceilidhContext = new CobbleContext();
            foreach (var register in impl)
                register.RegisterUnits(ceilidhContext);

            ceilidhContext.Execute();
        }
    }
}
