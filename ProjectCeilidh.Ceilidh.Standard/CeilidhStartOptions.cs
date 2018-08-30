namespace ProjectCeilidh.Ceilidh.Standard
{
    public sealed class CeilidhStartOptions
    {
        public string[] StartOptions { get; }

        public CeilidhStartOptions(string[] startOptions)
        {
            StartOptions = startOptions;
        }
    }
}
