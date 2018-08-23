using System;
using System.Text;

namespace Ceilidh.Core.Util
{
    public static class EncodingExtensions
    {
        public static Span<byte> GetBytesNullTerminated(this Encoding enc, ReadOnlySpan<char> str)
        {
            Span<byte> data = new byte[enc.GetByteCount(str) + 1];
            enc.GetBytes(str, data.Slice(0, data.Length - 1));
            data[data.Length - 1] = 0;
            return data;
        }

        public static string GetStringNullTerminated(this Encoding enc, ReadOnlySpan<byte> data) => enc.GetString(data.Slice(0, data.IndexOf((byte)0)));
    }
}
