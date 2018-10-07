using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    public sealed class PortAudioContext : IDisposable
    {
        private readonly bool _shouldTerminate;

        private PortAudioContext()
        {
            var err = Pa_Initialize();
            if (err < PaErrorCode.NoError)
            {
                _shouldTerminate = false;
                throw new PortAudioException(err);
            }
            _shouldTerminate = true;
        }

        private void ReleaseUnmanagedResources()
        {
            if (_shouldTerminate) Pa_Terminate();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PortAudioContext()
        {
            ReleaseUnmanagedResources();
        }

        public static PortAudioContext EnterContext()
        {
            return new PortAudioContext();
        }

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_Initialize();

        [DllImport(PortAudio.LIBRARY_NAME)]
        private static extern PaErrorCode Pa_Terminate();
    }
}
