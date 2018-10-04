using System;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public abstract class OutputDevice : IDisposable
    {
        /// <summary>
        /// The name of the output device.
        /// </summary>
        public abstract string Name { get; }
        public abstract IOutputController Controller { get; }
        public abstract bool IsDefault { get; }

        public abstract PlaybackHandle Init(AudioStream stream);

        public virtual void Dispose() { }
    }
}