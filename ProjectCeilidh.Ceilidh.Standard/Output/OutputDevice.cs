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
        /// <summary>
        /// Name of the API used to access this device.
        /// </summary>
        public abstract IOutputController Controller { get; }

        public abstract void Play(AudioStream stream);

        public virtual void Dispose() { }
    }
}