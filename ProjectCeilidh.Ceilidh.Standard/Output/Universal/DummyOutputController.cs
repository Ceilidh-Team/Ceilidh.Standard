using System;
using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Localization;

namespace ProjectCeilidh.Ceilidh.Standard.Output.Universal
{
    // [CobbleExport]
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

        public IEnumerable<IOutputDevice> GetOutputDevices()
        {
            yield return new DummyOutputDevice(this, _localization);
        }

        private class DummyOutputDevice : IOutputDevice
        {
            public string Name { get; }
            public IOutputController Controller { get; }
            public bool IsDefault => true;

            public DummyOutputDevice(IOutputController controller, ILocalizationController localization)
            {
                Name = localization.Translate(DUMMY_NAME_KEY);
                Controller = controller;
            }

            public IPlaybackHandle Init(AudioStream stream)
            {
                return new DummyPlaybackHandle(stream);
            }

            public void Dispose() { }
        }

        private class DummyPlaybackHandle : IPlaybackHandle
        {
            public AudioStream BaseStream { get; }

            public DummyPlaybackHandle(AudioStream baseStream)
            {
                BaseStream = baseStream;
            }
            
            public void Start()
            {
                throw new NotImplementedException();
            }

            public void Seek(TimeSpan position)
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                BaseStream.Dispose();
            }

            public event PlaybackEndEventHandler PlaybackEnd;
        }
    }
}
