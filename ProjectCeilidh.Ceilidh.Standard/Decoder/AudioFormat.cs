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

        public override string ToString()
        {
            return $"{SampleRate} Hz, {Channels}Ch, {DataFormat}";
        }
    }

    public enum NumberFormat : byte
    {
        Unsigned,
        Signed,
        FloatingPoint
    }

    public readonly struct AudioDataFormat
    {
        public readonly NumberFormat NumberFormat;
        public readonly bool BigEndian;
        public readonly int BytesPerSample;

        public AudioDataFormat(NumberFormat numberFormat, bool bigEndian, int bytesPerSample)
        {
            NumberFormat = numberFormat;
            BigEndian = bigEndian;
            BytesPerSample = bytesPerSample;
        }

        public override int GetHashCode()
        {
            return ((byte)NumberFormat << 16) | (BigEndian ? 0x0000 : 0x1000) | BytesPerSample;
        }

        public override string ToString()
        {
            return $"{NumberFormat}, {(BigEndian ? "Big Endian" : "Little Endian")}, {BytesPerSample * 8}-bit";
        }

        /*Unsigned = 0x0000,
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
        Float64LittleEndian = FloatingPoint | LittleEndian | 64*/
    }
}
