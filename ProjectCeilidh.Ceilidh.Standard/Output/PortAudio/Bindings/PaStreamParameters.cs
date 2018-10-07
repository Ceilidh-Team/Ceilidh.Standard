using System;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaStreamParameters
    {
        public int DeviceIndex;
        public int ChannelCount;
        public PaSampleFormats SampleFormats;
        public PaTime SuggestedLatency;
        public IntPtr HostApiSpecificStreamInfo;
    }
}
