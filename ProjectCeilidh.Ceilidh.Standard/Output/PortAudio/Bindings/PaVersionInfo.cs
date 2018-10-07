using System;
using System.Runtime.InteropServices;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaVersionInfo
    {
        public string VersionControlRevision => _versionControlRevision == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(_versionControlRevision);

        public string VersionText => _versionText == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(_versionText);

        public Version Version => new Version(VersionMajor, VersionMinor, VersionSubMinor);

        public int VersionMajor;
        public int VersionMinor;
        public int VersionSubMinor;
        private IntPtr _versionControlRevision;
        private IntPtr _versionText;
    }
}
