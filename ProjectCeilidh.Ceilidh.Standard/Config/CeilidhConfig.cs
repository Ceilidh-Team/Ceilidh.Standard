using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace ProjectCeilidh.Ceilidh.Standard.Config
{
    /// <summary>
    /// The main CeilidhConfig structure
    /// </summary>
    [XmlRoot(ElementName = "CeilidhConfig", IsNullable = false)]
    public class CeilidhConfig
    {
        /// <summary>
        /// The default config value
        /// </summary>
        public static readonly CeilidhConfig DefaultConfig = new CeilidhConfig
        {
            Culture = CultureInfo.CurrentUICulture,
            ExcludeClass = new HashSet<string>(),
            Plugins = new List<string>()
        };

        /// <summary>
        /// Path to the profile directory, by default it's "~/.ceilidh"
        /// </summary>
        [XmlIgnore]
        public string HomePath;

        /// <summary>
        /// The name of the culture to use for localization
        /// </summary>
        [XmlElement(ElementName = "culture", IsNullable = false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string CultureName;

        /// <summary>
        /// The culture to use for localization
        /// </summary>
        [XmlIgnore]
        public CultureInfo Culture
        {
            get => CultureName == null ? null : CultureInfo.GetCultureInfo(CultureName);
            set => CultureName = value?.Name;
        }

        /// <summary>
        /// Classes in Ceilidh.Standard to exclude from registration
        /// </summary>
        [XmlArray(ElementName = "exclusions")]
        [XmlArrayItem("exclude")]
        public HashSet<string> ExcludeClass;

        /// <summary>
        /// A list of disk paths to plugins to load
        /// </summary>
        [XmlArray(ElementName = "plugins")]
        [XmlArrayItem("plugin")]
        public List<string> Plugins;
    }
}
