using System;

namespace ProjectCeilidh.Ceilidh.Standard.Mount
{
    public delegate void RecordChangedEventHandler(object sender, RecordChangedEventArgs e);

    public enum RecordChangeType
    {
        Change,
        Rename,
        Delete,
        Add
    }
    
    public class RecordChangedEventArgs : EventArgs
    {
        public RecordChangeType ChangeType { get; }
        public IMountRecord Record { get; }
    }
    
    public interface IMountWatch : IDisposable
    {
        Uri Path { get; }
        
        
    }
}