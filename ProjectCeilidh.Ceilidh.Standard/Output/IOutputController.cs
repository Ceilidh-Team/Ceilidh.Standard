using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public interface IOutputController
    {
        IEnumerable<OutputDevice> GetOutputDevices();
    }
}