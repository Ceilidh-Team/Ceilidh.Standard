using System;
using System.IO;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.Implementations.Ffmpeg
{
    public sealed class FfmpegDecoder : IDecoder
    {
        private readonly bool _supported;
        private readonly ILocalizationController _localization;

        public FfmpegDecoder(ILocalizationController localization)
        {
            _localization = localization;

            try
            {
                AvFormatContext.RegisterAllFormats();

                Console.WriteLine(_localization.Translate("libav.util.version", FfmpegVersion.AvUtilVersion));
                Console.WriteLine(_localization.Translate("libav.format.version", FfmpegVersion.AvFormatVersion));
                Console.WriteLine(_localization.Translate("libav.codec.version", FfmpegVersion.AvCodecVersion));

                _supported = true;
            }
            catch(TypeLoadException)
            {
                Console.WriteLine(_localization.Translate("libav.disabled"));

                _supported = false;
            }
        }

        public bool TryDecode(Stream source, out AudioData audioData)
        {
            audioData = null;
            if (!_supported) return false;

            AvIoContext io = null;
            try
            {
                AvFormatContext format = null;

                io = new AvIoContext(source);
                try
                {
                    format = new AvFormatContext(io);

                    if (format.OpenInput() != AvError.Ok || format.FindStreamInfo() != AvError.Ok)
                    {
                        format.Dispose();
                        return false;
                    }

                    var file = format.GetFileMetadata();
                    foreach (var (key, value) in file)
                        Console.WriteLine($"{key}: {value}");
                    
                    var data = format.GetStreamMetadata();

                    foreach (var stream in data)
                    foreach (var (key, value) in stream)
                        Console.WriteLine($"{key}: {value}");

                    return true;
                }
                catch
                {
                    io = null;
                    format?.Dispose();
                    throw;
                }
            }
            catch
            {
                io?.Dispose();
                throw;
            }
        }
    }
}
