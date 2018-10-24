using System;
using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountFile : IMountRecord
    {
        long? Length { get; }
        DateTime? DateCreated { get; }
        DateTime? DateModified { get; }

        bool TryOpenRead(out Stream stream);
        bool TryOpenWrite(out Stream stream);
    }
}