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
                dev[0].Play(new ReplayGainFilter().TransformAudioStream(data.GetAudioStream()));
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

            public override unsafe void Play(AudioStream stream)
            {
                var xAudio2 = new XAudio2(XAudio2Version.Version27);
                var masteringVoice = new MasteringVoice(xAudio2, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate, _deviceId);

                var format = WaveFormat.CreateCustomFormat(
                    stream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint
                        ? WaveFormatEncoding.IeeeFloat
                        : WaveFormatEncoding.Pcm, stream.Format.SampleRate, stream.Format.Channels,
                    stream.Format.Channels * stream.Format.SampleRate * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.Channels * stream.Format.DataFormat.BytesPerSample,
                    stream.Format.DataFormat.BytesPerSample * 8);

                var source = new SourceVoice(xAudio2, format, true);
                source.BufferEnd += BufferEnd;

                {
                    var buf = new byte[stream.Format.Channels * stream.Format.SampleRate *
                                       stream.Format.DataFormat.BytesPerSample];
                    var len = stream.Read(buf, 0, buf.Length);
                    fixed (byte* ptr = buf)
                        source.SubmitSourceBuffer(new AudioBuffer(new DataPointer(ptr, len)), null);
                }

                source.Start();

                while (true) ;

                void BufferEnd(IntPtr context)
                {
                    var buf = new byte[stream.Format.Channels * stream.Format.SampleRate * stream.Format.DataFormat.BytesPerSample];
                    var len = stream.Read(buf, 0, buf.Length);
                    if (len <= 0)
                    {
                        source.Stop();
                        source.DestroyVoice();
                        source.Dispose();
                        source.BufferEnd -= BufferEnd;

                        masteringVoice.DestroyVoice();
                        masteringVoice.Dispose();

                        xAudio2.Dispose();
                        return;
                    }

                    fixed (byte* ptr = buf)
                        source.SubmitSourceBuffer(new AudioBuffer(new DataPointer(ptr, len)), null);
                }
            }

            public override void Dispose()
            {

            }
        }
    }
}
