using System;
using System.Runtime.InteropServices;
using ProjectCeilidh.Ceilidh.Standard.Decoder;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal static class PortAudio
    {
        public const string LIBRARY_NAME = "portaudio";

        public static Version Version
        {
            get
            {
                int num = Pa_GetVersion();
                return new Version((num >> 16) & 0xFF, (num >> 8) & 0xFF, num & 0xFF);
            }
        }

        public static extern int ApiCount
        {
            [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetHostApiCount")]
            get;
        }

        public static extern int DefaultApiIndex
        {
            [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetDefaultHostApi")]
            get;
        }

        public static extern int DeviceCount
        {
            [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetDeviceCount")]
            get;
        }

        public static extern int DefaultInputDevice
        {
            [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetDefaultInputDevice")]
            get;
        }

        public static extern int DefaultOutputDevice
        {
            [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetDefaultOutputDevice")]
            get;
        }

        public static PaSampleFormats GetSampleFormat(this AudioDataFormat format)
        {
            switch (format.BytesPerSample)
            {
                case 1 when format.NumberFormat == NumberFormat.Signed:
                    return PaSampleFormats.Int8;
                case 1 when format.NumberFormat == NumberFormat.Unsigned:
                    return PaSampleFormats.UInt8;
                case 2:
                    return PaSampleFormats.Int16;
                case 3:
                    return PaSampleFormats.Int24;
                case 4 when format.NumberFormat == NumberFormat.Signed:
                    return PaSampleFormats.Int32;
                case 4 when format.NumberFormat == NumberFormat.FloatingPoint:
                    return PaSampleFormats.Float32;
                default:
                    throw new NotSupportedException();
            }
        }

        [DllImport(LIBRARY_NAME)]
        private static extern int Pa_GetVersion();

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetErrorText")]
        public static extern string GetErrorText(PaErrorCode errorCode);

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetHostApiInfo")]
        public static extern ref PaHostApiInfo GetHostApiInfo(int hostApiIndex);

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_HostApiTypeIdToHostApiIndex")]
        public static extern int HostTypeIdToIndex(PaHostApiTypeId type);

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_HostApiDeviceIndexToDeviceIndex")]
        public static extern int HostApiDeviceIndexToDeviceIndex(int hostApiIndex, int hostApiDeviceIndex);

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetLastHostErrorInfo")]
        public static extern ref PaHostErrorInfo GetLastHostErrorInfo();

        [DllImport(LIBRARY_NAME, EntryPoint = "Pa_GetDeviceInfo")]
        public static extern ref PaDeviceInfo GetDeviceInfo(int deviceIndex);

        [DllImport(LIBRARY_NAME)]
        private unsafe static extern PaErrorCode Pa_IsFormatSupported(PaStreamParameters* inputParameters, PaStreamParameters* outputParameters, double sampleRate);
    }
}
