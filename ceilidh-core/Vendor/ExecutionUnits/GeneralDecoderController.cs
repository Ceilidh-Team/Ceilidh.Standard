using System.Collections.Generic;
using System.IO;
using Ceilidh.Core.Vendor.Contracts;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    public class GeneralDecoderController : IDecoderController
    {
        private readonly IReadOnlyList<IDecoder> _decoders;

        public GeneralDecoderController(IEnumerable<IDecoder> decoders)
        {
            _decoders = new List<IDecoder>(decoders);
        }

        public bool TryDecode(LowTrack track, out AudioData audioData)
        {
            foreach (var decoder in _decoders)
            {
                Stream lastStream = null;
                try
                {
                    if (decoder.TryDecode(lastStream = track.GetStream(), out audioData))
                        return true;
                }
                catch
                {
                    lastStream?.Dispose();
                    throw;
                }

                lastStream.Dispose();
            }

            audioData = null;
            return false;
        }
    }
}
