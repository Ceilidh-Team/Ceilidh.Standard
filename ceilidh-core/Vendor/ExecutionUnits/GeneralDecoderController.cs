using System.Collections.Generic;
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

        public bool TryDecode(LowTrack track, out AudioStream audioData)
        {
            audioData = null;
            return false;
        }
    }
}
