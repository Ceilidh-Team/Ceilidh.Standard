namespace ProjectCeilidh.Ceilidh.Standard.Output
{
    public abstract class OutputDevice
    {
        /// <summary>
        /// The name of the output device.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Name of the API used to access this device.
        /// </summary>
        public abstract string Api { get; }
    }
}