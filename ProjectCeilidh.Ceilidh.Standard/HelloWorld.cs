using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Ceilidh.Standard.Debug;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Library;
using ProjectCeilidh.Ceilidh.Standard.Localization;
using ProjectCeilidh.Ceilidh.Standard.Output;

namespace ProjectCeilidh.Ceilidh.Standard
{
    [CobbleExport]
    public class HelloWorld
    {
        public HelloWorld(CeilidhConfig config, ILocalizationController localization, IDebugOutputController debug, IDecoderController decoder, ILibraryController library, IEnumerable<IOutputController> output)
        {
            debug.WriteLine(localization.Translate("hello", "Ceilidh"), DebugMessageLevel.Info);

            debug.WriteLine($"Home path: {config.HomePath}", DebugMessageLevel.Info);

            foreach (var controller in output)
                debug.WriteLine($"Found audio API: {controller.ApiName}", DebugMessageLevel.Info);

            /*if (library.TryGetSource("", out var source) && decoder.TryDecode(source, out var audioData) &&
                audioData.TrySelectStream(0))
            {
                
            }*/
        }
    }
}
