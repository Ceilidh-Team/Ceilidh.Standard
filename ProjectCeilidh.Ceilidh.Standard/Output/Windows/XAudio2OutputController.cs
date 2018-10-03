using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Filter;
using ProjectCeilidh.Ceilidh.Standard.Library;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ProjectCeilidh.Ceilidh.Standard.Output.Windows
{
    [CobbleExport(nameof(OSPlatform.Windows))]
    public class XAudio2OutputController : IOutputController
    {
        public string ApiName => "XAudio2";

        private readonly XAudio2 _xAudio2;

        public XAudio2OutputController(ILibraryController library, IDecoderController decoder)
        {
            _xAudio2 = new XAudio2(XAudio2Version.Version27);

            var dev = GetOutputDevices().ToArray();

            if (library.TryGetSource(@"G:\Audiophile Library\Perturbator\PERTURBATOR - Cult of Luna & Julie Christmas - Cygnus (Perturbator remix).flac", out var source) && decoder.TryDecode(source, out var data) &&
                data.TrySelectStream(0))
            {
                dev[0].Init(new ReplayGainFilter().TransformAudioStream(data.GetAudioStream())).Start();
            }
        }

        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            for (var i = 0; i < _xAudio2.DeviceCount; i++)
                yield return new XAudio2OutputDevice(this, i, _xAudio2.GetDeviceDetails(i));
        }

        private class XAudio2OutputDevice : OutputDevice
        {
            public override string Name { get; }
            public override IOutputController Controller { get; }

            private readonly int _deviceId;

            public XAudio2OutputDevice(IOutputController controller, int deviceId, DeviceDetails details)
            {
                Name = details.DisplayName;
                Controller = controller;

                _deviceId = deviceId;
            }

            public override PlaybackHandle Init(AudioStream stream) => new XAudio2PlaybackHandle(_deviceId, stream);

            public override void Dispose()
            {

            }
        }
        
        private class XAudio2PlaybackHandle : PlaybackHandle
        {
            public override AudioStream BaseStream { get; }
            public override long SamplesPlayed => _sourceVoice.State.SamplesPlayed;

            private readonly XAudio2 _xAudio2;
            private readonly MasteringVoice _masteringVoice;
            private readonly SourceVoice _sourceVoice;
            private readonly byte[] _audioBuffer;
            
            public XAudio2PlaybackHandle(int deviceId, AudioStream stream)
            {
                _xAudio2 = new XAudio2(XAudio2Version.Version27);
                _masteringVoice = new MasteringVoice(_xAudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceId);
                _sourceVoice = new SourceVoice(_xAudio2, WaveFormat.CreateCustomFormat(
                    stream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint
                        ? WaveFormatEncoding.IeeeFloat
                        : WaveFormatEncoding.Pcm, stream.Format.SampleRate, stream.Format.Channels,
                    stream.Format.Channels * stream.Format.SampleRate * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.Channels * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.DataFormat.BytesPerSample * 8), true);
                
                _sourceVoice.BufferEnd += QueueBufferData;

                BaseStream = stream;
                _audioBuffer = new byte[stream.Format.SampleRate * stream.Format.Channels * stream.Format.DataFormat.BytesPerSample];
                
                QueueBufferData();
                QueueBufferData();
            }

            private void QueueBufferData() => QueueBufferData(IntPtr.Zero);
            private unsafe void QueueBufferData(IntPtr _)
            {
                var len = BaseStream.Read(_audioBuffer, 0, _audioBuffer.Length);
                if (len <= 0)
                {
                    _sourceVoice.BufferEnd -= QueueBufferData;
                    _sourceVoice.Discontinuity();
                    PlaybackEnd?.Invoke(this, EventArgs.Empty);
                    return;
                }

                fixed (byte* ptr = _audioBuffer)
                    _sourceVoice.SubmitSourceBuffer(new AudioBuffer(new DataPointer(ptr, len)), null);
            }

            public override void Start()
            {
                _sourceVoice.Start();
            }

            public override void Seek(TimeSpan position)
            {
                _sourceVoice.Stop();
                _sourceVoice.BufferEnd -= QueueBufferData;
                _sourceVoice.FlushSourceBuffers();
                BaseStream.Seek(position);

                _sourceVoice.BufferEnd += QueueBufferData;
                
                QueueBufferData();
                QueueBufferData();
                _sourceVoice.Start();
            }

            public override void Stop()
            {
                _sourceVoice.Stop();
            }

            public override void Dispose()
            {
                _sourceVoice.BufferEnd -= QueueBufferData;
                
                _xAudio2.Dispose();
                _masteringVoice.Dispose();
                _sourceVoice.Dispose();
                BaseStream.Dispose();
            }

            public override event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
