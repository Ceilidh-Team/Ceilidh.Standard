using System.Reflection;
using System.Runtime.CompilerServices;
using Ceilidh.Core.Plugin.Attributes;

[assembly: PluginAssembly]

[assembly: AssemblyCompany("Ceilidh")]
[assembly: AssemblyVersion("0.0.*")]
[assembly: AssemblyInformationalVersion("v0.0.0-alpha")]
[assembly: AssemblyProduct("Ceilidh")]
[assembly: AssemblyDescription("Core module for Ceilidh.")]
[assembly: AssemblyTitle("Ceilidh Core")]

[assembly: InternalsVisibleTo("ceilidh-core-tests")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif