using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Library;

namespace ProjectCeilidh.Ceilidh.Standard.Decoder
{
    [CobbleExport]
    public class GeneralDecoderController : IDecoderController
    {
        private readonly ConcurrentBag<IDecoder> _decoders;

        public GeneralDecoderController(IEnumerable<IDecoder> decoders)
        {
            _decoders = new ConcurrentBag<IDecoder>(decoders);
        }

        public bool TryDecode(Source source, out AudioData audioData)
        {
            foreach (var decoder in _decoders)
            {
                Stream lastStream = default;
                try
                {
                    if (decoder.TryDecode(lastStream = source.GetStream(), out audioData))
                        return true;
                }
                catch
                {
                    lastStream?.Dispose();
                    throw;
                }

                lastStream.Dispose();
            }

            audioData = default;
            return false;
        }

        public void UnitLoaded(IDecoder decoder) => _decoders.Add(decoder);
    }
}
