using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
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

        public XAudio2OutputController()
        {
            _xAudio2 = new XAudio2(XAudio2Version.Version27);
        }

        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            var res = new XAudio2OutputDevice[_xAudio2.DeviceCount];

            for (var i = 0; i < _xAudio2.DeviceCount; i++)
                res[i] = new XAudio2OutputDevice(this, i, _xAudio2.GetDeviceDetails(i));

            res[res.Length - 1] = new XAudio2OutputDevice(this, -1, res.Take(res.Length - 1).Single(x => x.Details.Role.HasFlag(DeviceRole.DefaultMultimediaDevice)).Details);

            return res;
        }

        private class XAudio2OutputDevice : OutputDevice
        {
            public override string Name { get; }
            public override IOutputController Controller { get; }
            public override bool IsDefault => _deviceId == -1;
            public DeviceDetails Details { get; }

            private readonly int _deviceId;

            public XAudio2OutputDevice(IOutputController controller, int deviceId, DeviceDetails details)
            {
                Name = details.DisplayName;
                Controller = controller;
                Details = details;

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

            private readonly XAudio2 _xAudio2;
            private readonly MasteringVoice _masteringVoice;
            private readonly SourceVoice _sourceVoice;
            private volatile bool _playing;
            private volatile bool _ending;

            public XAudio2PlaybackHandle(int deviceId, AudioStream stream)
            {
                if (deviceId == -1)
                {
                    _xAudio2 = new XAudio2();
                    _masteringVoice = new MasteringVoice(_xAudio2);
                }
                else
                {
                    _xAudio2 = new XAudio2(XAudio2Version.Version27);
                    _masteringVoice = new MasteringVoice(_xAudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, deviceId);
                }
                
                _sourceVoice = new SourceVoice(_xAudio2, WaveFormat.CreateCustomFormat(
                    stream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint
                        ? WaveFormatEncoding.IeeeFloat
                        : WaveFormatEncoding.Pcm, stream.Format.SampleRate, stream.Format.Channels,
                    stream.Format.Channels * stream.Format.SampleRate * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.Channels * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.DataFormat.BytesPerSample * 8), true);
                
                _sourceVoice.BufferEnd += PushBuffer;

                BaseStream = stream;
            }

            private unsafe void PushBuffer(IntPtr context)
            {
                if (context != IntPtr.Zero)
                    Marshal.FreeHGlobal(context);

                if (_ending) PlaybackEnd?.Invoke(this, EventArgs.Empty);

                if (!_playing) return;

                var buf = new byte[BaseStream.Format.BytesPerFrame * 44100];
                var len = BaseStream.Read(buf, 0, buf.Length);

                if (len <= 0)
                {
                    _sourceVoice.Discontinuity();
                    _playing = false;
                    _ending = true;
                    return;
                }

                var hGlobal = Marshal.AllocHGlobal(len);
                fixed (byte* ptr = buf)
                    Buffer.MemoryCopy(ptr, hGlobal.ToPointer(), len, len);

                _sourceVoice.SubmitSourceBuffer(new AudioBuffer(new DataPointer(hGlobal, len)) {Context = hGlobal},
                    null);
            }

            public override void Start()
            {
                _playing = true;
                _sourceVoice.Start();

                if (_sourceVoice.State.BuffersQueued == 0)
                {
                    PushBuffer(IntPtr.Zero);
                    PushBuffer(IntPtr.Zero);
                }
            }

            public override void Seek(TimeSpan position)
            {
                _playing = false;
                _sourceVoice.Stop();
                _sourceVoice.FlushSourceBuffers();
                BaseStream.Seek(position);

                while (_sourceVoice.State.BuffersQueued != 0) { } // spinwait

                _playing = true;

                PushBuffer(IntPtr.Zero);
                PushBuffer(IntPtr.Zero);

                _sourceVoice.Start();
            }

            public override void Stop()
            {
                _playing = false;
                _sourceVoice.Stop();
            }

            public override void Dispose()
            {
                _sourceVoice.BufferEnd -= PushBuffer;
                
                _xAudio2.Dispose();
                _masteringVoice.Dispose();
                _sourceVoice.Dispose();
                BaseStream.Dispose();
            }

            public override event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
