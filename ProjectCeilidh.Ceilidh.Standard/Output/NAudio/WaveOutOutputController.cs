using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using NAudio;
using NAudio.Wave;

namespace ProjectCeilidh.Ceilidh.Standard.Output.NAudio
{
    [CobbleExport]
    public class WaveOutOutputController : IOutputController
    {
        public WaveOutOutputController()
        {
            
        }
        
        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            for (var i = 0; i < WaveOut.DeviceCount; i++)
                yield return new NAudioOutputDevice(new WaveOut
                {
                    DeviceNumber = i
                }, WaveOut.GetCapabilities(i).ProductName);
        }
    }
}