using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Tools
{
    public unsafe class ReplayGainScanner
    {
        public Version LibEbur128Version
        {
            get
            {
                ebur128_get_version(out var major, out var minor, out var patch);
                return new Version(major, minor, patch);
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
        private static extern int ebur128_set_channel(State* state, uint channelNumber, Channel value);

        [DllImport("libebur128")]
        private static extern int ebur128_add_frames_short(State* state, short* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern int ebur128_add_frames_int(State* state, int* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern int ebur128_add_frames_float(State* state, float* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern int ebur128_add_frames_double(State* state, double* src, IntPtr frames);

        [DllImport("libebur128")]
        private static extern int ebur128_loudness_global(State* state, out double loudness);

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
            M = 1 << 0,
            S = (1 << 1) | M,
            I = (1 << 2) | M,
            Lra = (1 << 3) | S,
            SamplePeak = (1 << 4) | M,
            TruePeak = (1 << 5) | M | SamplePeak,
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
