using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaDeviceInfo
    {
        public string Name => _name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(_name);

        public int StructVersion;
        private IntPtr _name;
        public int HostApiIndex;
        public int MaxInputChannels;
        public int MaxOutputChannels;
        public PaTime DefaultLowInputLatency;
        public PaTime DefaultLowOutputLatency;
        public PaTime DefaultHighInputLatency;
        public PaTime DefaultHighOutputLatency;
        public double DefaultSampleRate;
    }
}