using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    /// <summary>
    /// A container object that describes how to access source data at a low level.
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// The URI for this source.
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Get a stream for the specified source.
        /// </summary>
        /// <returns>A readable stream containing source data.</returns>
        Stream GetStream();
    }
}
