using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaHostErrorInfo
    {
        public string ErrorText => _errorText == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(_errorText);

        public PaHostApiTypeId HostApiType;
        public long ErrorCode;
        private IntPtr _errorText;
    }
}
