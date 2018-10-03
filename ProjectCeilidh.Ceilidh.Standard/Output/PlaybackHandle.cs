using System;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public abstract class PlaybackHandle : IDisposable
    {
        public delegate void PlaybackEndEventHandler(object source, EventArgs e);

        public abstract AudioStream BaseStream { get; }
        public abstract long SamplesPlayed { get; }
        public TimeSpan PlaybackPosition => TimeSpan.FromSeconds(SamplesPlayed / (double) BaseStream.Format.SampleRate);
        
        public abstract void Start();
        public abstract void Seek(TimeSpan position);
        public abstract void Stop();
        
        public abstract void Dispose();

        public abstract event PlaybackEndEventHandler PlaybackEnd;
    }
}