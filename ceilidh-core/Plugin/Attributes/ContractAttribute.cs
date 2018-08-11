using System;

namespace Ceilidh.Core.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ContractAttribute : Attribute
    {
        public bool Singleton { get; set; }
    }
}