namespace ProjectCeilidh.Ceilidh.Standard.DebugOutput
{
    public interface IDebugOutputController
    {
        void WriteLine(string message, DebugMessageLevel level);
    }
}
