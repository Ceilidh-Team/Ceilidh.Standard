using System.IO;

namespace ProjectCeilidh.Ceilidh.Standard.Library
{
    /// <summary>
    /// A container object that describes how to access source data at a low level.
    /// </summary>
    public abstract class Source
    {
        /// <summary>
        /// The URI for this source.
        /// </summary>
        public string Uri { get; protected set; }

        /// <summary>
        /// Get a stream for the specified source.
        /// </summary>
        /// <returns>A readable stream containing source data.</returns>
        public abstract Stream GetStream();

        public override int GetHashCode() => Uri?.GetHashCode() ?? 0;
        public override string ToString() => Uri;
        public override bool Equals(object obj) => obj is Source s && s.Uri == Uri;
        public virtual bool Equals(Source source) => source.Uri == Uri;

        public static bool operator ==(Source one, Source two) => (one == null && two == null) || one.Equals(two);
        public static bool operator !=(Source one, Source two) => !(one == two);
    }
}
