using System;
using ProjectCeilidh.Ceilidh.Standard.DebugOutput;

namespace ProjectCeilidh.Ceilidh.ConsoleShell
{
    public class ConsoleOutputConsumer : IDebugOutputConsumer
    {
        public void WriteLine(string message, DebugMessageLevel level)
        {
            switch (level)
            {
                case DebugMessageLevel.Verbose:
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[VERBOSE] ");
                    Console.ResetColor();
                    break;
                }
                case DebugMessageLevel.Info:
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("[INFO] ");
                    Console.ResetColor();
                    break;
                }
                case DebugMessageLevel.Warning:
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[WARNING] ");
                    Console.ResetColor();
                    break;
                }
                case DebugMessageLevel.Error:
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[ERROR] ");
                    Console.ResetColor();
                    break;
                }
                case DebugMessageLevel.Fatal:
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("[FATAL] ");
                    Console.ResetColor();
                    break;
                }
            }

            Console.WriteLine(message);
        }
    }
}
