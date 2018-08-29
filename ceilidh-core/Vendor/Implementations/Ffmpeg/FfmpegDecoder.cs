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

                Console.WriteLine(_localization.Translate("ffmpeg.util.version", FfmpegVersion.AvUtilVersion));
                Console.WriteLine(_localization.Translate("ffmpeg.format.version", FfmpegVersion.AvFormatVersion));
                Console.WriteLine(_localization.Translate("ffmpeg.codec.version", FfmpegVersion.AvCodecVersion));

                _supported = true;

                /*if (TryDecode(File.OpenRead("/Users/olivia/Downloads/A Beautiful Song.ogg"), out var data))
                    using(data)
                    {
                        data.TrySelectStream(0);
                        using (var audio = data.GetAudioStream())
                        using (var file = File.OpenWrite("data.raw"))
                        {
                            audio.CopyTo(file);
                        }
                    }*/
            }
            catch(TypeLoadException)
            {
                Console.WriteLine(_localization.Translate("ffmpeg.disabled"));

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

                    audioData = new FfmpegAudioData(io, format);
                    io = null;
                    format = null;
                    
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
