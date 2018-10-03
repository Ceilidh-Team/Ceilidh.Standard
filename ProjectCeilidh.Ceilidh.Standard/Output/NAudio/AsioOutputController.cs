using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ProjectCeilidh.Ceilidh.Standard.Output.NAudio
{
    public class AsioOutputController : IOutputController
    {
        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            if (!AsioOut.isSupported()) yield break;
            foreach (var driver in AsioOut.GetDriverNames())
                yield return new NAudioOutputDevice(new AsioOut(driver), driver);
        }
    }
}