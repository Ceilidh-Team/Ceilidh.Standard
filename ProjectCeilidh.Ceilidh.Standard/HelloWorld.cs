using System;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard
{
    [CobbleExport]
    public class HelloWorld
    {
        public HelloWorld(CeilidhConfig config, ILocalizationController localization)
        {
            Console.WriteLine(localization.Translate("hello", "Ceilidh"));

            Console.WriteLine("Home path: {0}", config.HomePath);
        }
    }
}
