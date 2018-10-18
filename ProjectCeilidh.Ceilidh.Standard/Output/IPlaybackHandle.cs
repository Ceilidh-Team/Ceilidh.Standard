using System;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public delegate void PlaybackEndEventHandler(object source, EventArgs e);
    
    public interface IPlaybackHandle : IDisposable
    {
        AudioStream BaseStream { get; }
        
        void Start();
        void Seek(TimeSpan position);
        void Stop();
        
        event PlaybackEndEventHandler PlaybackEnd;
    }
}