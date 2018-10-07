using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PaStreamFinishedCallback(IntPtr userData);
}
