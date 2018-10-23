using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.PortAudio;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio
{
    internal class PortAudioOutputController : IOutputController, IDisposable
    {
        public string ApiName { get; }

        private readonly PortAudioHostApi _api;
        private readonly IReadOnlyCollection<PortAudioDevice> _outputDevices;
        public PortAudioOutputController(PortAudioHostApi api)
        {
            _api = api;
            ApiName = $"{Regex.Replace(api.Name, "^Windows ", "")} (PortAudio)";

            var devList = new List<PortAudioDevice>();
            foreach (var device in api.Devices)
            {
                if (device.MaxOutputChannels <= 0)
                {
                    device.Dispose();
                    continue;
                }

                devList.Add(device);
            }

            _outputDevices = devList;
        }

        public IEnumerable<IOutputDevice> GetOutputDevices()
        {
            PortAudioDevice def = default;
            try
            {
                def = _api.DefaultOutputDevice;
            }
            catch (PortAudioException)
            {
                //_debug.WriteLine($"Default output device for audio API \"{ApiName}\" does not exist.", DebugMessageLevel.Warning);
            }

            foreach (var device in _outputDevices)
                yield return new PortAudioOutputDevice(this, ReferenceEquals(def, device), device);
        }

        public void Dispose()
        {
            foreach (var device in _outputDevices)
                device.Dispose();

            _api.Dispose();
        }

        private class PortAudioOutputDevice : IOutputDevice
        {
            public string Name { get;}

            public IOutputController Controller { get; }

            public bool IsDefault { get; }

            private readonly PortAudioDevice _dev;

            public PortAudioOutputDevice(IOutputController controller, bool isDefault, PortAudioDevice device)
            {
                Name = device.Name;
                Controller = controller;
                IsDefault = isDefault;
                _dev = device;
            }

            public IPlaybackHandle Init(AudioStream stream) => new PortAudioPlaybackHandle(stream, _dev);

            public void Dispose() { }
        }

        private class PortAudioPlaybackHandle : IPlaybackHandle
        {
            public AudioStream BaseStream { get; }

            private volatile bool _isSeeking;
            private readonly PortAudioDevicePump _pump;

            public PortAudioPlaybackHandle(AudioStream baseStream, PortAudioDevice dev)
            {
                BaseStream = new BufferedAudioStream(baseStream, baseStream.Format.SampleRate * baseStream.Format.BytesPerFrame);

                PortAudioSampleFormat.PortAudioNumberFormat numberFormat;
                switch (baseStream.Format.DataFormat.NumberFormat)
                {
                    case NumberFormat.FloatingPoint:
                        numberFormat = PortAudioSampleFormat.PortAudioNumberFormat.FloatingPoint;
                        break;
                    case NumberFormat.Signed:
                        numberFormat = PortAudioSampleFormat.PortAudioNumberFormat.Signed;
                        break;
                    case NumberFormat.Unsigned:
                        numberFormat = PortAudioSampleFormat.PortAudioNumberFormat.Unsigned;
                        break;
                    default: throw new ArgumentException();
                }

                _pump = new PortAudioDevicePump(dev, baseStream.Format.Channels,
                    new PortAudioSampleFormat(numberFormat, baseStream.Format.DataFormat.BytesPerSample),
                    dev.DefaultHighOutputLatency, baseStream.Format.SampleRate, DataCallback);

                _pump.StreamFinished += PumpOnStreamFinished;
            }

            private void PumpOnStreamFinished(object sender, EventArgs e)
            {
                if (!_isSeeking) PlaybackEnd?.Invoke(this, EventArgs.Empty);
            }

            private int DataCallback(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

            public void Start() => _pump.Start();

            public void Seek(TimeSpan position)
            {
                _isSeeking = true;
                _pump.Abort();
                _pump.ClearBuffers();
                BaseStream.Seek(position);
                _pump.RestartAfterClear();
                _isSeeking = false;
            }

            public void Stop() => _pump.Stop();

            public void Dispose()
            {
                _pump.Dispose();
                BaseStream.Dispose();
            }

            public event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
