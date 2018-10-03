using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public interface IOutputController
    {
        string ApiName { get; }

        IEnumerable<OutputDevice> GetOutputDevices();
    }
}