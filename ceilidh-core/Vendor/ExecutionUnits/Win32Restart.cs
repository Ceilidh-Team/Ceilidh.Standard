using System;
using System.Runtime.InteropServices;
using Ceilidh.Core.Plugin.Attributes;

namespace Ceilidh.Core.Vendor.ExecutionUnits
{
    [ExecutionUnit(SupportedPlatforms = new[]
    {
        PlatformID.Win32NT
    })]
    public class Win32Restart
    {
        public Win32Restart()
        {
            RegisterApplicationRestart($"dotnet {Environment.CommandLine}", 0);
        }

        [DllImport("kernel32.dll")]
        private static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string pwzCommandLine, RegisterApplicationRestartFlags dwFlags);

        [Flags]
        private enum RegisterApplicationRestartFlags
        {
            /// <summary>
            /// Do not restart the process if it terminates due to an unhandled exception
            /// </summary>
            NoCrash = 1,
            /// <summary>
            /// Do not restart the process if it terminates due to the application not responding
            /// </summary>
            NoHang = 2,
            /// <summary>
            /// Do not restart the process if it terminates due to the installation of an update
            /// </summary>
            NoPatch = 4,
            /// <summary>
            /// Do not restart the process if the computer is restarted as the result of an update
            /// </summary>
            NoReboot = 8
        }
    }
}
