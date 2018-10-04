using System.Collections.Generic;

namespace ProjectCeilidh.Ceilidh.Standard.Filter.FFmpeg
{
    public class FilterConfiguration
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, string> Options { get; }

        public FilterConfiguration(string name, IReadOnlyDictionary<string, string> options)
        {
            Name = name;
            Options = options;
        }
    }
}
