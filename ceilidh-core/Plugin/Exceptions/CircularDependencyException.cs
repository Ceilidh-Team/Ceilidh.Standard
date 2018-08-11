using System;

namespace Ceilidh.Core.Plugin.Exceptions
{
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException()
        {
        }
    }
}