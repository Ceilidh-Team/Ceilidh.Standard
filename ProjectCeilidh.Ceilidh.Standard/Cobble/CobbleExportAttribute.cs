using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Cobble
{
    /// <summary>
    /// Marks a class to be registered with Cobble
    /// </summary>
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class)]
    internal class CobbleExportAttribute : Attribute
    {
        private readonly OSPlatform? _platform;

        public CobbleExportAttribute()
        {
            _platform = default;
        }

        public CobbleExportAttribute(string platform)
        {
            _platform = OSPlatform.Create(platform.ToUpperInvariant());
        }

        public bool IsPlatform()
        {
            return _platform == null || RuntimeInformation.IsOSPlatform(_platform.Value);
        }
    }
}
