using System;
using System.IO;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Decoder.FFmpeg;
using Xunit;

namespace ProjectCeilidh.Ceilidh.Standard.Tests
{
    public class DecoderTests
    {
        private readonly FFmpegDecoder _decoder;

        public DecoderTests()
        {
            _decoder = new FFmpegDecoder();
        }

        [Fact]
        public void TestProbe()
        {
            using (var file = File.OpenRead("BrownianNoise.flac"))
            {
                Assert.True(_decoder.TryDecode(file, out var audioData));
                using (audioData)
                {
                    Assert.Equal(1, audioData.StreamCount);
                    Assert.True(audioData.TrySelectStream(0));
                    using (var stream = audioData.GetAudioStream())
                    {
                        Assert.Equal(44100, stream.Format.SampleRate);
                        Assert.Equal(2, stream.Format.Channels);
                        Assert.Equal(NumberFormat.Signed, stream.Format.DataFormat.NumberFormat);
                        Assert.Equal(2, stream.Format.DataFormat.BytesPerSample);
                        Assert.Equal(1323000, stream.TotalSamples);
                        Assert.Equal(TimeSpan.FromSeconds(30), stream.Duration);
                    }
                }
            }
        }
    }
}
