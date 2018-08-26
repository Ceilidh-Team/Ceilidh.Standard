using System;
using System.Runtime.InteropServices;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    internal unsafe struct AvCodecParameters
    {
        public readonly AvMediaType CodecType;
        public readonly int CodecId;
        public readonly uint CodecTag;
        public readonly byte* ExtraData;
        public readonly int ExtraDataLength;
        public readonly int Format;
        public readonly long BitRate;
    }

    internal unsafe struct AvCodecContext
    {
        public Span<byte> ExtraData => new Span<byte>(_extraData, _extraDataSize);

#pragma warning disable 169
#pragma warning disable 649

        private readonly void* _avClass;
        private readonly int _logLevelOffset;
        public readonly AvMediaType CodecType;
        public readonly AvCodec* Codec;
        public readonly int CodecId;
        public readonly uint CodecTag;
        private readonly void* _privateData;
        private readonly void* _internal;
        public IntPtr Opaque;
        public long BitRate;
        public int BitRateTolerance;
        public int GlobalQuality;
        public int CompressionLevel;
        public int Flags;
        public int Flags2;

        private byte* _extraData;
        private int _extraDataSize;

        public AvRational TimeBase;
        public int TicksPerFrame;
        public int Delay;
        public int Width, Height;
        public int CodedWidth, CodedHeight;
        public int GopSize;
        public int PixelFormat;
        public IntPtr DrawHorizontalBand;
        public IntPtr GetFormat;
        public int MaxBFrames;
        public float BQuantFactor;
        public int BFrameStrategy;
        public float BQuantOffset;
        public int HasBFrames;
        public int MpegQuant;
        public float IQuantFactor;
        public float IQuantOffset;
        public float LumiMasking;
        public float TemporalCplxMasking;
        public float SpatialCplxMasking;
        public float PMasking;
        public float DarkMasing;
        public int SliceCount;
        public int PredictionMethod;
        public int* SliceOffset;
        public AvRational SampleAspectRatio;
        public int MeCmp;
        public int MeSubCmp;
        public int MbCmp;
        public int IldctCmp;
        public int DiaSize;
        public int LastPredictorCount;
        public int PreMe;
        public int MePreCmp;
        public int PreDiaSize;
        public int MeSubpelQuality;
        public int MeRange;
        public int SliceFlags;
        public int MbDecision;
        public ushort* IntraMatrix;
        public ushort* InterMatrix;
        public int ScenechangeThreshold;
        public int NoiseReduction;
        public int IntraDcPrecision;
        public int SkipTop;
        public int SkipBottom;
        public int MbLmin;
        public int MbLmax;
        public int MePenaltyCompensation;
        public int BidirRefine;
        public int BrdScale;
        public int KeyintMain;
        public int Refs;
        public int ChromaOffset;
        public int Mv0Threshold;
        public int BSensitivity;
        public int ColorPrimaries;
        public int ColorTrc;
        public int ColorSpace;
        public int ColorRange;
        public int ChromaSampleLocation;
        public int Slices;
        public int FieldOrder;
        public int SampleRate;
        public int Channels;
        public AvSampleFormat SampleFormat;

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