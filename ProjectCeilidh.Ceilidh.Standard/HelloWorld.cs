using System.Collections.Generic;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Ceilidh.Standard.DebugOutput;
using ProjectCeilidh.Ceilidh.Standard.Localization;
using ProjectCeilidh.Ceilidh.Standard.Output;

namespace ProjectCeilidh.Ceilidh.Standard
{
    [CobbleExport]
    public class HelloWorld
    {
        public HelloWorld(CeilidhConfig config, ILocalizationController localization, IDebugOutputController debug, IEnumerable<IOutputController> output)
        {
            debug.WriteLine(localization.Translate("hello", "Ceilidh"), DebugMessageLevel.Info);

            debug.WriteLine($"Home path: {config.HomePath}", DebugMessageLevel.Info);

            var outputs = output.ToArray();

            foreach (var controller in outputs)
                debug.WriteLine($"Found audio API: {controller.ApiName}", DebugMessageLevel.Info);
        }
    }
}
