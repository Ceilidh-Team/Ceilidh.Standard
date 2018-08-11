using System;
using System.Reflection;

namespace Ceilidh.Core.Plugin.Exceptions
{
    public class ExtraImplementationException : Exception
    {
        public readonly MemberInfo Contract;

        public ExtraImplementationException(Type contract)
        {
            Contract = contract;
        }
    }
}