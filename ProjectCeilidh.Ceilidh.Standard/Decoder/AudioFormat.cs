namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    /// <summary>
    /// Represents the parameters of an AudioStream.
    /// </summary>
    public struct AudioFormat
    {
        /// <summary>
        /// The number of bytes in a single frame of data (Channels * BytesPerSample)
        /// </summary>
        public int BytesPerFrame => Channels * DataFormat.BytesPerSample;
        /// <summary>
        /// The bitrate of the raw PCM data
        /// </summary>
        public int Bitrate => SampleRate * BytesPerFrame * 8;
        
        public readonly int SampleRate, Channels;
        /// <summary>
        /// The format of the underlying PCM data
        /// </summary>
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
    }
}
