using System.Collections.Generic;
using ProjectCeilidh.Ceilidh.Standard.Decoder;
using ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio
{
    internal class PortAudioOutputController : IOutputController
    {
        private readonly int _apiIndex;
        private readonly int _defaultDeviceIndex;

        public string ApiName
        {
            get;
        }

        public PortAudioOutputController(PaHostApiTypeId apiType)
        {
            _apiIndex = Bindings.PortAudio.HostTypeIdToIndex(apiType);
            ref var info = ref Bindings.PortAudio.GetHostApiInfo(_apiIndex);

            _defaultDeviceIndex = info.DefaultOutputDevice;
            ApiName = $"{info.Name} (PortAudio)";
        }

        public IEnumerable<OutputDevice> GetOutputDevices()
        {
            using (PortAudioContext.EnterContext())
            {
                var apiInfo = Bindings.PortAudio.GetHostApiInfo(_apiIndex);
                for (var i = 0; i < apiInfo.DeviceCount; i++)
                {
                    var index = Bindings.PortAudio.HostApiDeviceIndexToDeviceIndex(_apiIndex, i);
                    var info = Bindings.PortAudio.GetDeviceInfo(index);
                    if (info.MaxOutputChannels > 0)
                        yield return new PortAudioOutputDevice(this, index == _defaultDeviceIndex, index);
                }
            }
        }

        private class PortAudioOutputDevice : OutputDevice
        {
            private readonly int _deviceIndex;

            public override string Name
            {
                get;
            }

            public override IOutputController Controller
            {
                get;
            }

            public override bool IsDefault
            {
                get;
            }

            public PortAudioOutputDevice(IOutputController controller, bool isDefault, int deviceIndex)
            {
                Controller = controller;
                ref var deviceInfo = ref Bindings.PortAudio.GetDeviceInfo(deviceIndex);
                Name = deviceInfo.Name;
                IsDefault = isDefault;
                _deviceIndex = deviceIndex;
            }

            public override PlaybackHandle Init(AudioStream stream)
            {
                return new PaStream(stream, _deviceIndex);
            }
        }
    }
}
