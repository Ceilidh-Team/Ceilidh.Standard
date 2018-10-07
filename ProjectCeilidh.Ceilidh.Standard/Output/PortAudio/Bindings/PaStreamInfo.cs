namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaStreamInfo
    {
        public int StructVersion;
        public PaTime InputLatency;
        public PaTime OutputLatency;
        public double SampleRate;
    }
}
