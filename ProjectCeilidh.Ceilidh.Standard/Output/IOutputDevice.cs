using System;
using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public interface IOutputDevice : IDisposable
    {
        /// <summary>
        /// The name of the output device.
        /// </summary>
        string Name { get; }
        IOutputController Controller { get; }
        bool IsDefault { get; }

        IPlaybackHandle Init(AudioStream stream);
    }
}