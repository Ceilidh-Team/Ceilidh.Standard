using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ProjectCeilidh.Ceilidh.Standard.Output.NAudio
{
    public class WasapiOutputController : IOutputController
    {
        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                yield return new NAudioOutputDevice(new WasapiOut(device, AudioClientShareMode.Shared, true, 0), device.FriendlyName);
        }
    }
}