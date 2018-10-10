using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjectCeilidh.Ceilidh.Standard.Debug;
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

        public IEnumerable<OutputDevice> GetOutputDevices()
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

        private class PortAudioOutputDevice : OutputDevice
        {
            public override string Name { get;}

            public override IOutputController Controller { get; }

            public override bool IsDefault { get; }

            private readonly PortAudioDevice _dev;

            public PortAudioOutputDevice(IOutputController controller, bool isDefault, PortAudioDevice device)
            {
                Name = device.Name;
                Controller = controller;
                IsDefault = isDefault;
                _dev = device;
            }

            public override PlaybackHandle Init(AudioStream stream) => new PortAudioPlaybackHandle(stream, _dev);
        }

        private class PortAudioPlaybackHandle : PlaybackHandle
        {
            public override AudioStream BaseStream { get; }

            private volatile bool _isSeeking;
            private readonly PortAudioDevicePump _pump;

            public PortAudioPlaybackHandle(AudioStream baseStream, PortAudioDevice dev)
            {
                BaseStream = baseStream;

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
                    dev.DefaultLowOutputLatency, baseStream.Format.SampleRate, DataCallback);

                _pump.StreamFinished += PumpOnStreamFinished;
            }

            private void PumpOnStreamFinished(object sender, EventArgs e)
            {
                if (!_isSeeking) PlaybackEnd?.Invoke(this, EventArgs.Empty);
            }

            private int DataCallback(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

            public override void Start() => _pump.Start();

            public override void Seek(TimeSpan position)
            {
                _isSeeking = true;
                _pump.Abort();
                _pump.ClearBuffers();
                BaseStream.Seek(position);
                _pump.RestartAfterClear();
                _isSeeking = false;
            }

            public override void Stop() => _pump.Stop();

            public override void Dispose() => _pump.Dispose();

            public override event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
