using System;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Cobble;
using ProjectCeilidh.PortAudio;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio
{
    public class PortAudioUnitLoader : IUnitLoader
    {
        public void RegisterUnits(CobbleContext context)
        {
            try
            {
                foreach (var api in PortAudioHostApi.SupportedHostApis)
                    context.AddUnmanaged(new PortAudioOutputController(api));
            }
            catch (DllNotFoundException)
            {
                // I'm ignoring this because if it's not supported, we can just exclude it from the cobble context
            }
            catch (PortAudioException)
            {
                // Ditto
            }
        }
    }
}
