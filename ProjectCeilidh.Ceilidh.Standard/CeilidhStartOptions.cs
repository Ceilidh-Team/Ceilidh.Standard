namespace ProjectCeilidh.Ceilidh.Standard
{
    /// <summary>
    /// Options passed to Ceilidh on startup
    /// </summary>
    public sealed class CeilidhStartOptions
    {
        /// <summary>
        /// The path to the configuration file
        /// </summary>
        public string ConfigFile { get; set; }
        /// <summary>
        /// The path to the main profile folder
        /// </summary>
        public string UserDataPath { get; set; }
    }
}
