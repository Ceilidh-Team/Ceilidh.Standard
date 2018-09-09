using System;
using System.IO;
using System.Linq;
using Mono.Options;
using ProjectCeilidh.Ceilidh.Standard;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.ConsoleShell
{
    public static class ConsoleShell
    {
        private static int Main(string[] args)
        {
            var startOptions = new CeilidhStartOptions();

            var help = false;
            var options = new OptionSet
            {
                { "c|config=", "the path to the config file.", path => startOptions.ConfigFile = path },
                { "u|user=", "the path to the user data folder.", path => startOptions.UserDataPath = path },
                { "h|help", "show this message and exit.", _ => help = true }
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine("Argument error: {0}", e.Message);
                Console.WriteLine($"Try '{Path.GetFileName(Environment.GetCommandLineArgs()[0])} --help' for more information.");
                return -1;
            }

            if (help)
            {

                Console.WriteLine($"Ceilidh Console Shell Version {typeof(ConsoleShell).Assembly.GetName().Version}");
                Console.WriteLine($"Ceilidh Standard Version {typeof(IUnitLoader).Assembly.GetName().Version}");
                Console.WriteLine("Usage:");

                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            var loadContext = new CobbleContext();

            loadContext.AddUnmanaged(startOptions);

            foreach (var unit in typeof(IUnitLoader).Assembly.GetExportedTypes()
                .Where(x => x != typeof(IUnitLoader) && typeof(IUnitLoader).IsAssignableFrom(x)))
                loadContext.AddManaged(unit);
            loadContext.ExecuteAsync().Wait();
            if (!loadContext.TryGetImplementations<IUnitLoader>(out var impl)) return 0;

            var ceilidhContext = new CobbleContext();
            foreach (var register in impl)
                register.RegisterUnits(ceilidhContext);

            ceilidhContext.ExecuteAsync().Wait();

            return 0;
        }
    }
}
