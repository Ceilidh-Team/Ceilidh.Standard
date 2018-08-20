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

#if DEBUG
[assembly: InternalsVisibleTo("ceilidh-core-tests")]
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif