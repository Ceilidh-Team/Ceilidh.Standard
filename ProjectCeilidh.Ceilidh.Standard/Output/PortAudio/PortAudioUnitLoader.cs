using System;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings;
using ProjectCeilidh.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio
{
    public class PortAudioUnitLoader : IUnitLoader
    {
        public void RegisterUnits(CobbleContext context)
        {
            try
            {
                using (PortAudioContext.EnterContext())
                {
                    var apiCount = Bindings.PortAudio.ApiCount;
                    for (var i = 0; i < apiCount; i++)
                    {
                        var hostApiInfo = Bindings.PortAudio.GetHostApiInfo(i);
                        context.AddUnmanaged(new PortAudioOutputController(hostApiInfo.Type));
                    }
                }
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
