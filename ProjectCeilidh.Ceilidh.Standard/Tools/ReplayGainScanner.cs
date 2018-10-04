using System;
using System.IO;
using System.Runtime.InteropServices;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Filter;

namespace ProjectCeilidh.Ceilidh.Standard.Tools
{
    public static unsafe class ReplayGainScanner
    {
        private const double TARGET_LUFS = -18;
        
        public static Version LibEbur128Version
        {
            get
            {
                ebur128_get_version(out var major, out var minor, out var patch);
                return new Version(major, minor, patch);
            }
        }

        public static bool IsAvailable
        {
            get
            {
                try
                {
                    return LibEbur128Version.Major == 1;
                }
                catch (DllNotFoundException)
                {
                    return false;
                }
            }
        }

        public static Decibel GetSuggestedGain(AudioStream stream, double targetLufs = TARGET_LUFS)
        {
            if (!IsAvailable) throw new Exception();
            
            var state = ebur128_init((uint) stream.Format.Channels, (uint) stream.Format.SampleRate, Modes.Global);
            try
            {
                var buf = new byte[stream.Format.BytesPerFrame * stream.Format.SampleRate];
                int len;
                fixed (byte* ptr = buf)
                    while ((len = stream.Read(buf, 0, buf.Length)) > 0)
                    {
                        Error err;
                        switch (stream.Format.DataFormat.BytesPerSample)
                        {
                            case 2:
                                err = ebur128_add_frames_short(state, (short*) ptr, new IntPtr(len / stream.Format.BytesPerFrame));
                                break;
                            case 4 when stream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint:
                                err = ebur128_add_frames_float(state, (float*) ptr, new IntPtr(len / stream.Format.BytesPerFrame));
                                break;
                            case 4 when stream.Format.DataFormat.NumberFormat == NumberFormat.Signed:
                                err = ebur128_add_frames_int(state, (int*) ptr, new IntPtr(len / stream.Format.BytesPerFrame));
                                break;
                            case 8 when stream.Format.DataFormat.NumberFormat == NumberFormat.FloatingPoint:
                                err = ebur128_add_frames_double(state, (double*) ptr, new IntPtr(len / stream.Format.BytesPerFrame));
                                break;
                            default:
                                throw new InvalidDataException();
                        }
                        
                        if (err != Error.Success)
                            throw new InvalidDataException();
                    }

                if (ebur128_loudness_global(state, out var loudness) != Error.Success)
                    throw new InvalidDataException();
                
                return new Decibel(targetLufs - loudness);
            }
            finally
            {
                ebur128_destroy(ref state);
            }
        }
        
        #region Native

        [DllImport("libebur128")]
        private static extern void ebur128_get_version(out int major, out int minor, out int patch);

        [DllImport("libebur128")]
        private static extern State* ebur128_init(uint channels, ulong sampleRate, Modes mode);

        [DllImport("libebur128")]
        private static extern void ebur128_destroy(ref State* state);

        [DllImport("libebur128")]
        private static extern Error ebur128_set_channel(State* state, uint channelNumber, Channel value);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_short(State* state, short* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_int(State* state, int* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_float(State* state, float* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_double(State* state, double* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_loudness_global(State* state, out double loudness);

        private enum Channel
        {
            Unused = 0,
            Left = 1,
            Mp030 = 1,
            Right = 2,
            Mm030 = 2,
            Center = 3,
            Mp000 = 3,
            LeftSurround = 4,
            Mp110 = 4,
            RightSurround = 5,
            Mm110 = 5,
            DualMono,
            MpSc,
            MmSc,
            Mp060,
            Mm060,
            Mp090,
            Mp135,
            Mm135,
            Mp180,
            Up000,
            Up030,
            Um045,
            Up090,
            Um090,
            Up110,
            Um110,
            Up135,
            Um135,
            Up180,
            Tp000,
            Bp000,
            Bp045,
            Bm045
        }

        private enum Error
        {
            Success = 0,
            NoMem,
            InvalidMode,
            InvalidChannelIndex,
            NoChange
        }

        [Flags]
        private enum Modes
        {
            Momentary = 1 << 0,
            ShortTerm = (1 << 1) | Momentary,
            Global = (1 << 2) | Momentary,
            LoudnessRange = (1 << 3) | ShortTerm,
            SamplePeak = (1 << 4) | Momentary,
            TruePeak = (1 << 5) | Momentary | SamplePeak,
            Histogram = 1 << 6
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct State
        {
            public Modes Mode;
            public uint Channels;
            public ulong SampleRate;
            private IntPtr _internal;
        }

        #endregion
    }
}
