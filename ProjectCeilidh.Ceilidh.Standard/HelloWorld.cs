using System;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard
{
    [CobbleExport]
    public class HelloWorld
    {
        public HelloWorld(CeilidhStartOptions options)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine(string.Join(", ", options.StartOptions));
        }
    }
}
