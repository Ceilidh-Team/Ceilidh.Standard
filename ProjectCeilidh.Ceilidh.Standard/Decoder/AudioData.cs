using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public abstract class AudioData : IDisposable
    {
        public abstract IReadOnlyDictionary<string, string> Metadata { get; }
        public abstract int StreamCount { get; }
        public abstract int SelectedStream { get; }

        public abstract bool TrySelectStream(int streamIndex);
        public abstract AudioStream GetAudioStream();

        public abstract void Dispose();
    }
}
