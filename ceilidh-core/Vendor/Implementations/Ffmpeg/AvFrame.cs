using System;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal unsafe struct AvFrame
    {
        private byte* _data1;
        private byte* _data2;
        private byte* _data3;
        private byte* _data4;
        private byte* _data5;
        private byte* _data6;
        private byte* _data7;
        private byte* _data8;
        public fixed int LineSize[8];
        public byte** ExtendedData;
        public int Width, Height;
        public int SampleCount;
        public AvSampleFormat Format;
    }
}
