using System;
using System.Collections.Generic;
using System.Linq;

namespace Ceilidh.Core.Util
{
    internal class PluginImplementationMap
    {
        private readonly IReadOnlyDictionary<Type, List<object>> _impl;

        public PluginImplementationMap(IReadOnlyDictionary<Type, List<object>> impl)
        {
            _impl = impl;
        }

        public bool TryGetSingleton<TContract>(out TContract single)
        {
            single = default(TContract);

            if (!_impl.TryGetValue(typeof(TContract), out var values)) return false;

            var res = values.SingleOrDefault();
            if (res == null) return false;
            single = (TContract) res;
            return true;
        }

        public bool TryGetImplementations<TContract>(out List<TContract> impl)
        {
            impl = default(List<TContract>);

            if (!_impl.TryGetValue(typeof(TContract), out var list)) return false;

            impl = list.OfType<TContract>().ToList();
            return true;
        }
    }
}