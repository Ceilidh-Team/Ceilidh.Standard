using System;
using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Cobble;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard.Output.Universal
{
    [CobbleExport]
    public class DummyOutputController : IOutputController
    {
        internal const string DUMMY_NAME_KEY = "output.dummy";
        internal const string DUMMY_API_NAME_KEY = "output.dummy.api";

        public string ApiName { get; }

        private readonly ILocalizationController _localization;

        public DummyOutputController(ILocalizationController localization)
        {
            ApiName = localization.Translate(DUMMY_API_NAME_KEY);

            _localization = localization;
        }

        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            yield return new DummyOutputDevice(_localization);
        }

        private class DummyOutputDevice : OutputDevice
        {
            public override string Name { get; }
            public override IOutputController Controller { get; }

            public DummyOutputDevice(ILocalizationController localization)
            {
                Name = localization.Translate(DUMMY_NAME_KEY);
            }

            public override PlaybackHandle Init(AudioStream stream)
            {
                return new DummyPlaybackHandle();
            }

            public override void Dispose() { }
        }

        private class DummyPlaybackHandle : PlaybackHandle
        {
            public override AudioStream BaseStream { get; }

            public override void Start()
            {
                throw new NotImplementedException();
            }

            public override void Seek(TimeSpan position)
            {
                throw new NotImplementedException();
            }

            public override void Stop()
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            public override event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
