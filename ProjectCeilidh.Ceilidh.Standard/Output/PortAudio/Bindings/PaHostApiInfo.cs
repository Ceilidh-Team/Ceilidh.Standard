using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaHostApiInfo
    {
        public string Name => _name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(_name);

        public int StructVersion;
        public PaHostApiTypeId Type;
        private IntPtr _name;
        public int DeviceCount;
        public int DefaultInputDevice;
        public int DefaultOutputDevice;
    }
}
