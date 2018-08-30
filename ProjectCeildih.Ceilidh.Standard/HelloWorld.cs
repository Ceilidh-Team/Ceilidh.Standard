using System;
using ProjectCeildih.Ceilidh.Standard.Cobble;

namespace ProjectCeildih.Ceilidh.Standard
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
