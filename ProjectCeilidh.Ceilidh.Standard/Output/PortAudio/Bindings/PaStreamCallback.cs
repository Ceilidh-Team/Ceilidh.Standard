using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate PaStreamCallbackResult PaStreamCallback(IntPtr input, IntPtr output, ulong frameCount, in PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData);
}
