using System;
using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Localization;
using ProjectCeilidh.Ceilidh.Standard.Tools;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    /// <summary>
    /// Implement ReplayGain volume normalization dynamically as data arrives
    /// </summary>
    [CobbleExport]
    public class ReplayGainAdaptiveFilter : IFilterProvider
    {
        public string Name { get; }

        public ReplayGainAdaptiveFilter(ILocalizationController localizationController)
        {
            Name = localizationController.Translate("filter.replaygain.adaptive");
        }
        
        public AudioStream TransformAudioStream(AudioStream stream) => new ReplayGainAdaptiveAudioStream(stream, new Decibel(ReplayGainScanner.TARGET_LUFS));

        private unsafe class ReplayGainAdaptiveAudioStream : AudioStream
        {
            private const double ALPHA = .5, MAX_DB = 0;
            
            public override bool CanSeek => _baseAudioStream.CanSeek;
            public override long Position
            {
                get => _baseAudioStream.Position;
                set => _baseAudioStream.Position = value;
            }
            public override AudioFormat Format => _baseAudioStream.Format;
            public override long TotalSamples => _baseAudioStream.TotalSamples;
            
            private Decibel _currentGain;
            private readonly AudioStream _baseAudioStream;
            private readonly Decibel _target;
            private readonly EbuR128State _ebuR128State;
            private readonly Action<byte[], int, int, double> _transformFunc;
            
            public ReplayGainAdaptiveAudioStream(AudioStream baseAudioStream, Decibel target) : base(baseAudioStream.ParentData)
            {
                _currentGain = new Decibel(0);
                _baseAudioStream = baseAudioStream;
                _target = target;
                _ebuR128State = new EbuR128State(baseAudioStream.Format, EbuR128Modes.ShortTerm);

                switch (baseAudioStream.Format.DataFormat.BytesPerSample)
                {
                    case 2:
                        _transformFunc = TransformShort;
                        break;
                    case 4 when baseAudioStream.Format.DataFormat.NumberFormat == NumberFormat.Signed:
                        _transformFunc = TransformInt;
                        break;
                    case 4 when baseAudioStream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint:
                        _transformFunc = TransformFloat;
                        break;
                    case 8:
                        _transformFunc = TransformDouble;
                        break;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var len = _baseAudioStream.Read(buffer, offset, count);
                _ebuR128State.AddFrames(buffer, offset, len);
                _currentGain = new Decibel(Math.Min(MAX_DB, _currentGain.Value * ALPHA + (1 - ALPHA) * (_target.Value - _ebuR128State.GetLoudness())));
                // Console.WriteLine("Current Gain: {0} dB", _currentGain.Value);

                _transformFunc(buffer, offset, len, _currentGain.GetAmplitudeRatio());

                return len;
            }

            public override void Seek(TimeSpan timestamp)
            {
                _baseAudioStream.Seek(timestamp);
            }

            #region Transform

            private static void TransformShort(byte[] buffer, int offset, int len, double ratio)
            {
                fixed (byte* ptr = &buffer[offset])
                    for (var i = 0; i < len / sizeof(short); i++)
                        ((short*) ptr)[i] = (short) (((short*) ptr)[i] * ratio);
            }
            
            private static void TransformInt(byte[] buffer, int offset, int len, double ratio)
            {
                fixed (byte* ptr = &buffer[offset])
                    for (var i = 0; i < len / sizeof(int); i++)
                        ((int*) ptr)[i] = (int) (((int*) ptr)[i] * ratio);
            }
            
            private static void TransformFloat(byte[] buffer, int offset, int len, double ratio)
            {
                fixed (byte* ptr = &buffer[offset])
                    for (var i = 0; i < len / sizeof(float); i++)
                        ((float*) ptr)[i] = (float) (((float*) ptr)[i] * ratio);
            }
            
            private static void TransformDouble(byte[] buffer, int offset, int len, double ratio)
            {
                fixed (byte* ptr = &buffer[offset])
                    for (var i = 0; i < len / sizeof(double); i++)
                        ((double*) ptr)[i] *= ratio;
            }

            #endregion
        }
    }
}