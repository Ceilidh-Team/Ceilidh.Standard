using CommandLine;

namespace Ceilidh.Core.Config
{
    public class CeilidhArguments
    {
        [Option('c', "config", Default = null, HelpText = "Specify the path to the configuration file",
            Required = false)]
        public string ConfigFile { get; set; }

        [Option('u', "user", Default = null, HelpText = "Specify where user data is stored on disk.")]
        public string UserDataPath { get; set; }
    }
}