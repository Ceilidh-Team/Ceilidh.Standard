using System;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public interface IMountRecord : IDisposable
    {
        string Name { get; }
        Uri Path { get; }
        IMountDriver ParentDriver { get; }
        
        void Refresh();
    }
}