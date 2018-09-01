using System;

namespace ProjectCeilidh.Ceilidh.Standard.Cobble
{
    /// <summary>
    /// Marks a class to be registered with Cobble
    /// </summary>
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class)]
    internal class CobbleExportAttribute : Attribute
    {

    }
}
