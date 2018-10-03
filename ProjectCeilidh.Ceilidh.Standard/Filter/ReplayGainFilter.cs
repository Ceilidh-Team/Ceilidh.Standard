using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Filter
{
    public class ReplayGainFilter : IFilterProvider
    {
        public string Name => "ReplayGain";

        public ReplayGainFilter()
        {

        }

        public AudioStream TransformAudioStream(AudioStream stream)
        {
            return new ReplayGainAudioStream(stream);
        }

        private class ReplayGainAudioStream : AudioStream
        {
            private const string DB_REGEX = @"(?<value>-?\d+(?:\.\d+)?) dB";

            public override bool CanSeek => _baseStream.CanSeek;
            public override long Position
            {
                get => _baseStream.Position;
                set => _baseStream.Position = value;
            }
            public override AudioFormat Format => _baseStream.Format;
            public override long TotalSamples => _baseStream.TotalSamples;

            private readonly AudioStream _baseStream;
            private readonly Decibel _combinedDb;

            public ReplayGainAudioStream(AudioStream baseStream) : base(baseStream.ParentData)
            {
                _baseStream = baseStream;

                var db = new Decibel(0);

                if (baseStream.ParentData.Metadata.TryGetValue("REPLAYGAIN_ALBUM_GAIN", out var albumGainString))
                    db = new Decibel(double.Parse(Regex.Match(albumGainString, DB_REGEX).Groups["value"].Value));
                if (baseStream.ParentData.Metadata.TryGetValue("REPLAYGAIN_TRACK_GAIN", out var trackGainString))
                    db = new Decibel(double.Parse(Regex.Match(trackGainString, DB_REGEX).Groups["value"].Value));

                _combinedDb = db;
            }

            public override unsafe int Read(byte[] buffer, int offset, int count)
            {
                var len = _baseStream.Read(buffer, offset, count);

                fixed (byte* buf = &buffer[offset])
                {
                    switch (Format.DataFormat.NumberFormat)
                    {
                        case NumberFormat.FloatingPoint:
                            switch (Format.DataFormat.BytesPerSample)
                            {
                                case 4:
                                    for (var i = 0; i < len / 4; i++)
                                        ((float*) buf)[i] =
                                            (float) (((float*) buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 8:
                                    for (var i = 0; i < len / 8; i++)
                                        ((double*)buf)[i] *= _combinedDb.GetAmplitudeRatio();
                                    break;
                            }

                            break;
                        case NumberFormat.Signed:
                            switch (Format.DataFormat.BytesPerSample)
                            {
                                case 2:
                                    for (var i = 0; i < len / 2; i++)
                                        ((short*)buf)[i] =
                                            (short)(((short*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 4:
                                    for (var i = 0; i < len / 4; i++)
                                        ((int*)buf)[i] =
                                            (int)(((int*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 8:
                                    for (var i = 0; i < len / 8; i++)
                                        ((long*)buf)[i] =
                                            (long)(((long*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                            }
                            break;
                        case NumberFormat.Unsigned:
                            switch (Format.DataFormat.BytesPerSample)
                            {
                                case 1:
                                    for (var i = 0; i < len; i++)
                                        buf[i] =
                                            (byte)(((float*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 2:
                                    for (var i = 0; i < len / 2; i++)
                                        ((ushort*)buf)[i] =
                                            (ushort)(((ushort*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 4:
                                    for (var i = 0; i < len / 4; i++)
                                        ((uint*)buf)[i] =
                                            (uint)(((uint*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                                case 8:
                                    for (var i = 0; i < len / 8; i++)
                                        ((ulong*)buf)[i] =
                                            (ulong)(((ulong*)buf)[i] * _combinedDb.GetAmplitudeRatio());
                                    break;
                            }
                            break;
                    }
                }

                return len;
            }

            public override void Seek(TimeSpan timestamp)
            {
                _baseStream.Seek(timestamp);
            }

            protected override void Dispose(bool disposing)
            {
                _baseStream.Dispose();

                base.Dispose(disposing);
            }
        }

        private struct Decibel
        {
            public double Value { get; }

            public Decibel(double value)
            {
                Value = value;
            }

            [Pure]
            public double GetAmplitudeRatio() => Math.Pow(10, Value / 20);

            [Pure]
            public static Decibel operator +(Decibel one, Decibel two)
            {
                return new Decibel(10 * Math.Log10(Math.Pow(10, one.Value / 10) + Math.Pow(10, two.Value / 10)));
            }
        }
    }
}
