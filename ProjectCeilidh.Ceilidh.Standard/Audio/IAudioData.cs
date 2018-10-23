using System;
using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Audio
{
    public interface IAudioData : IDisposable
    {
        IReadOnlyDictionary<string, string> Metadata { get; }
        int StreamCount { get; }
        int SelectedStream { get; }

        bool TrySelectStream(int streamIndex);
        AudioStream GetAudioStream();
    }
}
