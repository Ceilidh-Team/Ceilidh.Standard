using System.Collections.Generic;
using System.Linq;
using ProjectCeilidh.Ceilidh.Standard.Cobble;

namespace ProjectCeilidh.Ceilidh.Standard.DebugOutput
{
    [CobbleExport]
    public class GeneralDebugOutputController : IDebugOutputController
    {
        private readonly IReadOnlyCollection<IDebugOutputConsumer> _consumers;

        public GeneralDebugOutputController(IEnumerable<IDebugOutputConsumer> consumers)
        {
            _consumers = consumers.ToArray();
        }

        public void WriteLine(string message, DebugMessageLevel level)
        {
            foreach (var consumer in _consumers)
                consumer.WriteLine(message, level);
        }
    }
}
