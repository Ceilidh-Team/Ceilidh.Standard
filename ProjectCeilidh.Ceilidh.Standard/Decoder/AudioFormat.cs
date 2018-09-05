namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    public struct AudioFormat
    {
        public readonly int SampleRate, Channels;
        public readonly AudioDataFormat DataFormat;

        public AudioFormat(int sampleRate, int channels, AudioDataFormat dataFormat)
        {
            SampleRate = sampleRate;
            Channels = channels;
            DataFormat = dataFormat;
        }

        public void Deconstruct(out int sampleRate, out int channels, out AudioDataFormat dataFormat)
        {
            sampleRate = SampleRate;
            channels = Channels;
            dataFormat = DataFormat;
        }
    }

    public enum AudioDataFormat : ushort
    {
        Unsigned = 0x0000,
        Signed = 0x0100,
        FloatingPoint = 0x0200,
        BigEndian = 0x0000,
        LittleEndian = 0x1000,

        Unsigned8 = Unsigned | 8,
        Signed8 = Signed | 8,
        Signed16BigEndian = Signed | BigEndian | 16,
        Signed16LittleEndian = Signed | LittleEndian | 16,
        Signed24BigEndian = Signed | BigEndian | 24,
        Signed24LittleEndian = Signed | LittleEndian | 24,
        Signed32BigEndian = Signed | BigEndian | 32,
        Signed32LittleEndian = Signed | LittleEndian | 32,
        Float32BigEndian = FloatingPoint | BigEndian | 32,
        Float32LittleEndian = FloatingPoint | LittleEndian | 32,
        Float64BigEndian = FloatingPoint | BigEndian | 64,
        Float64LittleEndian = FloatingPoint | LittleEndian | 64
    }

    public static class AudioDataFormatExtensions
    {
        public static AudioDataFormat Endianness(this AudioDataFormat format)
        {
            return (AudioDataFormat) ((ushort) format & 0xF000);
        }

        public static AudioDataFormat NumericType(this AudioDataFormat format)
        {
            return (AudioDataFormat) ((ushort) format & 0x0F00);
        }

        public static int BytesPerSample(this AudioDataFormat format)
        {
            return (ushort)format & 0x00FF;
        }
    }
}
