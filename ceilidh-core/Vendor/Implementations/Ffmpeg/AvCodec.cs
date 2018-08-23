using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal unsafe struct AvCodecContext
    {
#pragma warning disable 169
#pragma warning disable 649

        private readonly void* _avClass;
        private readonly int _logLevelOffset;
        public readonly int CodecType;
        public readonly AvCodec* Codec;
        public readonly int CodecId;

#pragma warning restore 169
#pragma warning restore 649
    }
    
    internal unsafe struct AvCodec
    {
        public string Name => Marshal.PtrToStringUTF8(new IntPtr(_name));
        public string LongName => Marshal.PtrToStringUTF8(new IntPtr(_longName));

        public ReadOnlySpan<AvRational> SupportedFramerates
        {
            get
            {
                var i = 0;
                var ptr = _supportedFramerates;
                while (ptr->Denominator != 0 && ptr->Numerator != 0)
                {
                    ptr++;
                    i++;
                }
                
                return new ReadOnlySpan<AvRational>(_supportedFramerates, i);
            }
        }
        
#pragma warning disable 169
#pragma warning disable 649

        private readonly byte* _name;
        private readonly byte* _longName;
        public readonly int Type;
        public readonly int Id;
        public readonly int Capabilities;
        private readonly AvRational* _supportedFramerates;

#pragma warning restore 169
#pragma warning restore 649
    }
}