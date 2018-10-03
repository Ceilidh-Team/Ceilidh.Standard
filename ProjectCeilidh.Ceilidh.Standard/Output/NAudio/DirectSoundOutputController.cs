using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace ProjectCeilidh.Ceilidh.Standard.Output.NAudio
{
    public class DirectSoundOutputController : IOutputController
    {
        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            return DirectSoundOut.Devices.Select(x => new NAudioOutputDevice(new DirectSoundOut(x.Guid), x.ModuleName));
        }
    }
}