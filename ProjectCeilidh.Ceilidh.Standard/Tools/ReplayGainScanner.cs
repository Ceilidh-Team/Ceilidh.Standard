using System;
using System.IO;
using System.Runtime.InteropServices;
using ProjectCeilidh.Ceilidh.Standard.Audio;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Filter;

namespace ProjectCeilidh.Ceilidh.Standard.Tools
{
    public static class ReplayGainScanner
    {
        public const double TARGET_LUFS = -18;
        
        public static bool IsAvailable
        {
            get
            {
                try
                {
                    return EbuR128State.Version.Major == 1;
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

            using (var state = new EbuR128State(stream.Format, EbuR128Modes.Global))
            {
                var buf = new byte[stream.Format.BytesPerFrame * stream.Format.SampleRate];
                int len;
                while ((len = stream.Read(buf, 0, buf.Length)) > 0)
                    state.AddFrames(buf, 0, len);

                return new Decibel(targetLufs - state.GetLoudness());
            }
        }
    }

    [Flags]
    public enum EbuR128Modes
    {
        Momentary = 1 << 0,
        ShortTerm = (1 << 1) | Momentary,
        Global = (1 << 2) | Momentary,
        LoudnessRange = (1 << 3) | ShortTerm,
        SamplePeak = (1 << 4) | Momentary,
        TruePeak = (1 << 5) | Momentary | SamplePeak,
        Histogram = 1 << 6
    }
    
    internal unsafe class EbuR128State : IDisposable
    {
        public static Version Version
        {
            get
            {
                ebur128_get_version(out var major, out var minor, out var patch);
                return new Version(major, minor, patch);
            }
        }
        
        private State* _state;
        private readonly AudioFormat _format;

        public EbuR128State(AudioFormat format, EbuR128Modes mode)
        {
            _state = ebur128_init((uint) format.Channels, (uint) format.SampleRate, mode);
            _format = format;
        }

        public void AddFrames(byte[] data, int offset, int count)
        {
            fixed (byte* ptr = &data[offset])
                switch (_format.DataFormat.BytesPerSample)
                {
                    case 2:
                        ebur128_add_frames_short(_state, (short*) ptr, count / _format.BytesPerFrame);
                        break;
                    case 4 when _format.DataFormat.NumberFormat == NumberFormat.Signed:
                        ebur128_add_frames_int(_state, (int*) ptr, count / _format.BytesPerFrame);
                        break;
                    case 4 when _format.DataFormat.NumberFormat == NumberFormat.FloatingPoint:
                        ebur128_add_frames_float(_state, (float*) ptr, count / _format.BytesPerFrame);
                        break;
                    case 8 when _format.DataFormat.NumberFormat == NumberFormat.FloatingPoint:
                        ebur128_add_frames_double(_state, (double*) ptr, count / _format.BytesPerFrame);
                        break;
                    default:
                        throw new InvalidDataException();
                }
        }

        public double GetLoudness()
        {
            double loudness;
            switch (_state->Mode)
            {
                case EbuR128Modes.Momentary:
                    ebur128_loudness_momentary(_state, out loudness);
                    break;
                case EbuR128Modes.ShortTerm:
                    ebur128_loudness_shortterm(_state, out loudness);
                    break;
                case EbuR128Modes.Global:
                    ebur128_loudness_global(_state, out loudness);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return loudness;
        }

        public void Dispose()
        {
            if (_state == null) return;
            
            ebur128_destroy(ref _state);
        }
        
        #region Native

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct NativeInt
        {
            public readonly IntPtr Value;

            public NativeInt(IntPtr value)
            {
                Value = value;
            }
            
            public static implicit operator NativeInt(IntPtr value) => new NativeInt(value);
            public static implicit operator NativeInt(int value) => new NativeInt(new IntPtr(value));
            public static implicit operator NativeInt(long value) => new NativeInt(new IntPtr(value));
        }
        
        [StructLayout(LayoutKind.Sequential)]
        private struct State
        {
            public EbuR128Modes Mode;
        }
        
        private enum Error
        {
            Success = 0,
            NoMem,
            InvalidMode,
            InvalidChannelIndex,
            NoChange
        }
        
        [DllImport("libebur128")]
        private static extern void ebur128_get_version(out int major, out int minor, out int patch);

        [DllImport("libebur128")]
        private static extern State* ebur128_init(uint channels, ulong sampleRate, EbuR128Modes ebuR128Mode);

        [DllImport("libebur128")]
        private static extern void ebur128_destroy(ref State* state);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_short(State* state, short* src, NativeInt frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_int(State* state, int* src, NativeInt frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_float(State* state, float* src, NativeInt frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_add_frames_double(State* state, double* src, NativeInt frames);

        [DllImport("libebur128")]
        private static extern Error ebur128_loudness_global(State* state, out double loudness);
        
        [DllImport("libebur128")]
        private static extern Error ebur128_loudness_momentary(State* state, out double loudness);
        
        [DllImport("libebur128")]
        private static extern Error ebur128_loudness_shortterm(State* state, out double loudness);

        #endregion
    }
}
