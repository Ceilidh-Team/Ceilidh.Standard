using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard.Output.Universal
{
    /*[CobbleExport]
    public class DummyOutputController : IOutputController
    {
        internal const string DUMMY_NAME_KEY = "output.dummy";
        internal const string DUMMY_API_NAME_KEY = "output.dummy.api";

        private readonly ILocalizationController _localization;

        public DummyOutputController(ILocalizationController localization)
        {
            _localization = localization;
        }

        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            yield return new DummyOutputDevice(_localization);
        }

        private class DummyOutputDevice : OutputDevice
        {
            public override string Name { get; }
            public override string Api { get; }

            public DummyOutputDevice(ILocalizationController localization)
            {
                Name = localization.Translate(DUMMY_NAME_KEY);
                Api = localization.Translate(DUMMY_API_NAME_KEY);
            }

            public override void Play(AudioStream stream)
            {

            }

            public override void Dispose() { }
        }
    }*/
}
