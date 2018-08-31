using System;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Config;

namespace ProjectCeilidh.Ceilidh.Standard
{
    [CobbleExport]
    public class HelloWorld
    {
        public HelloWorld(CeilidhConfig config)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Home path: {0}", config.HomePath);
        }
    }
}
