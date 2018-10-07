using System;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal class PortAudioException : Exception
    {
        public PortAudioException(PaErrorCode errorCode)
            : base(PortAudio.GetErrorText(errorCode))
        {
        }
    }
}
