namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaStreamCallbackTimeInfo
    {
        public PaTime InputBufferAdcTime;
        public PaTime CurrentTime;
        public PaTime OutputBufferDacTime;
    }
}
