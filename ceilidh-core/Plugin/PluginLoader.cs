using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceilidh.Core.Plugin.Attributes;
using Ceilidh.Core.Plugin.Exceptions;
using Ceilidh.Core.Util;

namespace Ceilidh.Core.Plugin
{
    internal class PluginLoader
    {
        /// <summary>
        ///     Cache a reference to the <see cref="Enumerable.OfType{TResult}" /> method so that we can create properly typed
        ///     IEnumerables for dependency injection.
        /// </summary>
        private readonly MethodInfo _ofType;

        /// <summary>
        ///     This set contains all the assemblies who's contracts and implementations will be loaded when <see cref="Execute" />
        ///     is called.
        /// </summary>
        private readonly HashSet<Assembly> _pluginAssemblies = new HashSet<Assembly>();

        private readonly List<object> _transientObjects;

        public PluginLoader(params object[] transientObjects)
        {
            _ofType = typeof(Enumerable).GetMethod(nameof(Enumerable.OfType));
            _transientObjects = transientObjects.Where(x => x != null).ToList();
        }

        /// <summary>
        ///     Execute the plugin load operation, invoking constructors for all execution units
        /// </summary>
        public PluginImplementationMap Execute(IEnumerable<string> excludeClasses)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
            try
            {
                var exclude = new HashSet<string>(excludeClasses);

                // Buffers used to hold all loaded contracts, execution units, and satisfying types
                var contracts = new HashSet<Type>();
                var executionUnits = new HashSet<Type>();

                var addedNames = new HashSet<string>(_pluginAssemblies.Select(x => x.GetName().FullName));
                var queue = new Queue<Assembly>(_pluginAssemblies);
                _pluginAssemblies.Clear();

                while (queue.Count > 0)
                {
                    var asm = queue.Dequeue();

                    // Look at every assembly we depend on, and queue up the ones that are plugins
                    foreach (var referencedAssembly in asm.GetReferencedAssemblies().Select(Assembly.Load)
                        .Where(x => x.GetCustomAttribute<PluginAssemblyAttribute>() != null))
                        if (addedNames.Add(referencedAssembly.GetName().FullName))
                            queue.Enqueue(referencedAssembly);

                    // Locate every contract in the assembly
                    foreach (var contract in asm.GetExportedTypes()
                        .Where(x => x.GetCustomAttribute<ContractAttribute>() != null))
                        contracts.Add(contract);

                    // Locate every execution unit in the assembly
                    foreach (var exUnit in asm.GetExportedTypes()
                        .Where(x => x.GetCustomAttribute<ExecutionUnitAttribute>() != null ||
                                    x.GetInterfaces().Any(y => y.GetCustomAttribute<ContractAttribute>() != null))
                        .Where(x => !exclude.Contains(x.FullName)))
                        executionUnits.Add(exUnit);
                }

                // Create a dependency graph linking every execution unit
                var graph = new DirectedGraph<Type>(executionUnits);
                foreach (var executionUnit in executionUnits)
                foreach (var dependency in GetDependencies(executionUnit)
                    .SelectMany(x => executionUnits.Where(x.IsAssignableFrom)))
                    graph.Link(dependency, executionUnit);

                var instances = new Dictionary<Type, List<object>>(contracts.Concat(executionUnits)
                    .Select(x => new KeyValuePair<Type, List<object>>(x, new List<object>())));

                foreach (var o in _transientObjects)
                    instances.Add(o.GetType(), new List<object>{o});

                // Iterate over the execution units after sorted topologically (no dependencies first)
                foreach (var exUnit in graph.TopologicalSort())
                {
                    // Create an instance and attach it to the contracts it implements
                    var inst = InstantiateExecutionUnit(exUnit, instances);
                    foreach (var implement in GetImplements(exUnit))
                    {
                        var attr = implement.GetCustomAttribute<ContractAttribute>();
                        var list = instances[implement];

                        if ((attr?.Singleton ?? false) && list.Count > 0)
                            throw new ExtraImplementationException(implement);

                        list.Add(inst);
                    }
                }

                return new PluginImplementationMap(instances);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveHandler;
            }

            // Serves to make assembly loading not version-specific
            Assembly ResolveHandler(object sender, ResolveEventArgs e)
            {
                var currentAsm = typeof(PluginLoader).Assembly;
                var currentName = currentAsm.GetName();

                var name = new AssemblyName(e.Name);

                switch (name)
                {
                    case var n when n.Name == currentName.Name && name.Version.AreEquivalent(currentName.Version):
                        return currentAsm;
                    default:
                    {
                        var asm = Assembly.ReflectionOnlyLoad(name.Name);
                        return name.Version.AreEquivalent(asm.GetName().Version) ? Assembly.Load(name.Name) : null;
                    }
                }
            }
        }

        /// <summary>
        ///     Queue an assembly for loading and execution when <see cref="Execute" /> is called.
        /// </summary>
        /// <param name="asm">The assembly to load</param>
        public void QueueLoad(Assembly asm)
        {
            _pluginAssemblies.Add(asm);
        }

        /// <summary>
        ///     Queue an assembly for loading and execution when <see cref="Execute" /> is called.
        /// </summary>
        /// <param name="path">The path to the assembly to load</param>
        public void QueueLoad(string path) => QueueLoad(Assembly.LoadFile(path));

        /// <summary>
        ///     Create an execution unit, drawing from the list of contract implementation instances
        /// </summary>
        /// <param name="exUnit">The type of the execution unit to create</param>
        /// <param name="instances">The set of instances</param>
        /// <returns>The instantiated execution unit</returns>
        private object InstantiateExecutionUnit(Type exUnit, IReadOnlyDictionary<Type, List<object>> instances)
        {
            var ctor = exUnit.GetConstructors().Single();
            return ctor.Invoke(ctor.GetParameters().Select(x =>
            {
                if (!x.ParameterType.IsConstructedGenericType ||
                    x.ParameterType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                    return instances.TryGetValue(x.ParameterType, out var b)
                        ? b.Single()
                        : null;

                var typ = x.ParameterType.GetGenericArguments()[0];

                return instances.TryGetValue(typ, out var a)
                    ? _ofType.MakeGenericMethod(x.ParameterType.GetGenericArguments())
                        .Invoke(null, new object[] {a})
                    : null;
            }).ToArray());
        }

        /// <summary>
        ///     Get the contracts that this execution unit implements
        /// </summary>
        /// <param name="exUnit">The execution unit type to get contracts for</param>
        /// <returns>An enumerable of contract types implemented</returns>
        private static IEnumerable<Type> GetImplements(Type exUnit)
        {
            return exUnit.GetInterfaces().Where(x => x.GetCustomAttribute<ContractAttribute>() != null).Append(exUnit);
        }

        /// <summary>
        ///     Given an execution unit, retrieve the contracts it depends on
        /// </summary>
        /// <param name="exUnit">The type for the execution unit</param>
        /// <returns>An enumerable of the contract types it depends on</returns>
        private static IEnumerable<Type> GetDependencies(Type exUnit)
        {
            return exUnit.GetConstructors().Single().GetParameters().Select(x =>
            {
                if (x.ParameterType.IsConstructedGenericType &&
                    x.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return x.ParameterType.GetGenericArguments()[0];

                return x.ParameterType;
            });
        }
    }
}