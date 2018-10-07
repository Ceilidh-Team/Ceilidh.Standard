using System;

namespace ProjectCeilidh.Ceilidh.Standard.Output.PortAudio.Bindings
{
    internal struct PaTime
    {
        private readonly double _value;

        public TimeSpan Value => TimeSpan.FromSeconds(_value);

        public PaTime(TimeSpan span)
        {
            _value = span.TotalSeconds;
        }
    }
}
