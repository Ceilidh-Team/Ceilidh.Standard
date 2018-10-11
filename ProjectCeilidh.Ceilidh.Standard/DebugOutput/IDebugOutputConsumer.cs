namespace ProjectCeilidh.Ceilidh.Standard.DebugOutput
{
    public interface IDebugOutputConsumer
    {
        void WriteLine(string message, DebugMessageLevel level);
    }
}
