using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace ProjectCeilidh.Ceilidh.Standard.Output.NAudio
{
    public class NAudioOutputDevice : OutputDevice
    {
        private static readonly IReadOnlyDictionary<Type, string> ApiNames = new Dictionary<Type, string>
        {
            [typeof(WaveOut)] = "WaveOut",
            [typeof(WasapiOut)] = "Wasapi",
            [typeof(DirectSoundOut)] = "DirectSound",
            [typeof(AsioOut)] = "Asio"
        };
        
        public override string Api => ApiNames.TryGetValue(_output.GetType(), out var apiName) ? apiName : "Unknown";
        public override string Name { get; }

        private readonly IWavePlayer _output;

        public NAudioOutputDevice(IWavePlayer output, string deviceName)
        {
            Name = deviceName;
            _output = output;
        }
    }
}