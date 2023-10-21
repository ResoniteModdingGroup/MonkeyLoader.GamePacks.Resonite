using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MonkeyLoader
{
    /// <summary>
    /// Manages a collection of <see cref="Assembly">Assemblies</see> and
    /// not yet <see cref="Assembly.Load(byte[])">loaded</see> <see cref="AssemblyDefinition"/>s.<br/>
    /// Handles <see cref="AppDomain.AssemblyResolve"/> events targeting its managed assemblies.
    /// </summary>
    public sealed class AssemblyPool
    {
        private readonly Dictionary<AssemblyName, AssemblyEntry> assemblies = new();

        /// <summary>
        /// Gets whether assemblies will be loaded when asked to resolve them.
        /// </summary>
        public bool LoadForResolve { get; }

        /// <summary>
        /// Creates a new <see cref="AssemblyPool"/> instance, loading assemblies when asked to resolve them if desired.
        /// </summary>
        /// <param name="loadForResolve">Whether to load assemblies when asked to resolve them.</param>
        public AssemblyPool(bool loadForResolve = true)
        {
            LoadForResolve = loadForResolve;
            AppDomain.CurrentDomain.AssemblyResolve += resolveAssembly;
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> for an entry without attempting to load it.
        /// </summary>
        /// <param name="name">The name of the <see cref="Assembly"/> to get.</param>
        /// <returns>The loaded <see cref="Assembly"/> if it was loaded already.</returns>
        /// <exception cref="InvalidOperationException">When the assembly hasn't been loaded yet.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public Assembly GetAssembly(AssemblyName name) => getEntry(name).GetAssembly();

        /// <summary>
        /// Loads all (not yet loaded) <see cref="Assembly"/> entries.
        /// </summary>
        public void LoadAll()
        {
            var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => new AssemblyName(assembly.GetName().Name)).ToHashSet();

            var entries = assemblies.Values
                .Where(entry => !entry.Loaded)
                .TopologicalSort(entry => entry.Name, entry => entry.GetDependencies(alreadyLoaded));

            foreach (var entry in entries)
                entry.LoadAssembly();
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> for an entry loading it if necessary.
        /// </summary>
        /// <param name="name">The name of the <see cref="Assembly"/> to get.</param>
        /// <returns>The loaded <see cref="Assembly"/>.</returns>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public Assembly LoadAssembly(AssemblyName name) => getEntry(name).LoadAssembly();

        /// <summary>
        /// Restores the <see cref="AssemblyDefinition"/> for an entry and releases its definition lock.
        /// </summary>
        /// <param name="name">The entry to restore.</param>
        /// <exception cref="InvalidOperationException">When the entry has already been loaded or returned.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public void RestoreDefinition(AssemblyName name) => getEntry(name).RestoreDefinition();

        /// <summary>
        /// Sets the given (new) <see cref="AssemblyDefinition"/> for and entry and releases its definition lock.
        /// </summary>
        /// <param name="name">The entry to set.</param>
        /// <param name="assemblyDefinition">The (new) <see cref="AssemblyDefinition"/>.</param>
        /// <exception cref="InvalidOperationException">When the entry has already been loaded or returned.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public void ReturnDefinition(AssemblyName name, AssemblyDefinition assemblyDefinition)
            => getEntry(name).ReturnDefinition(assemblyDefinition);

        /// <summary>
        /// Waits until nothing else is modifying the <see cref="AssemblyDefinition"/> of an entry anymore,
        /// before making a snapshot and returning it. The definition has to be returned using
        /// <see cref="RestoreDefinition"/> or <see cref="ReturnDefinition"/> exactly once.
        /// </summary>
        /// <param name="name">The entry to get.</param>
        /// <returns>The entry's <see cref="AssemblyDefinition"/>.</returns>
        /// <exception cref="InvalidOperationException">When the entry has already been loaded.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public AssemblyDefinition WaitForDefinition(AssemblyName name)
            => getEntry(name).WaitForDefinition();

        private AssemblyEntry getEntry(AssemblyName name)
        {
            if (!assemblies.TryGetValue(name, out var assemblyDefinition))
                throw new KeyNotFoundException($"No AssemblyDefinition loaded for [{name}]!");

            return assemblyDefinition;
        }

        private Assembly? resolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(AssemblyNameReference.Parse(args.Name).Name);

            if (!assemblies.TryGetValue(name, out var entry))
                return null;

            if (LoadForResolve)
                return entry.LoadAssembly();

            return entry.GetAssembly();
        }

        private sealed class AssemblyEntry : IDisposable
        {
            public readonly AssemblyName Name;
            private AssemblyDefinition? definition;
            private AutoResetEvent? definitionLock;
            private MemoryStream? definitionSnapshot;
            private bool disposedValue;
            private Assembly? loadedAssembly;

            [MemberNotNullWhen(true, nameof(loadedAssembly))]
            [MemberNotNullWhen(false, nameof(definition), nameof(definitionLock), nameof(definitionSnapshot))]
            public bool Loaded => loadedAssembly != null;

            public AssemblyEntry(AssemblyName name, AssemblyDefinition definition)
            {
                Name = name;
                this.definition = definition;
                definitionLock = new(true);
                definitionSnapshot = new();
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                dispose(true);
                GC.SuppressFinalize(this);
            }

            public Assembly GetAssembly()
            {
                if (!Loaded)
                    throw new InvalidOperationException($"Assembly for [{Name}] hasn't been loaded yet!");

                return loadedAssembly;
            }

            public IEnumerable<AssemblyName> GetDependencies(HashSet<AssemblyName> alreadyLoaded)
            {
                var fullNames = Loaded ?
                    loadedAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name)
                    : definition.Modules.SelectMany(module => module.AssemblyReferences)
                        .Select(reference => AssemblyNameReference.Parse(reference.Name).Name);

                return fullNames.Select(name => new AssemblyName(name))
                    .Where(name => !alreadyLoaded.Contains(name));
            }

            public Assembly LoadAssembly()
            {
                if (!Loaded)
                {
                    WaitForDefinition();
                    loadedAssembly = Assembly.Load(definitionSnapshot.ToArray());

                    definition.Dispose();
                    definition = null;

                    definitionSnapshot.Dispose();
                    definitionSnapshot = null;

                    definitionLock.Dispose();
                    definitionLock = null;
                }

                return loadedAssembly;
            }

            public void RestoreDefinition()
            {
                if (Loaded)
                    throwLoadedInvalidOperation();

                throwIfAlreadyReturned();

                definition.Dispose();
                definitionSnapshot.Position = 0;
                definition = AssemblyDefinition.ReadAssembly(definitionSnapshot);

                definitionLock.Set();
            }

            public void ReturnDefinition(AssemblyDefinition assemblyDefinition)
            {
                if (Loaded)
                    throwLoadedInvalidOperation();

                throwIfAlreadyReturned();

                definition = assemblyDefinition;
                definitionLock.Set();
            }

            public AssemblyDefinition WaitForDefinition()
            {
                if (Loaded)
                    throwLoadedInvalidOperation();

                definitionLock.WaitOne();

                definitionSnapshot.SetLength(0);
                definition.Write(definitionSnapshot);

                return definition;
            }

            private void dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing && !Loaded)
                    {
                        definition.Dispose();
                        definitionSnapshot.Dispose();
                        definitionLock.Dispose();
                    }

                    disposedValue = true;
                }
            }

            [DoesNotReturn]
            private void throwIfAlreadyReturned()
            {
                if (definitionLock!.WaitOne(0))
                {
                    definitionLock.Set();
                    throw new InvalidOperationException($"Can't return or restore AssemblyDefinition for [{Name}] when it wasn't taken first (using {nameof(WaitForDefinition)})!");
                }
            }

            [DoesNotReturn]
            private void throwLoadedInvalidOperation()
                => throw new InvalidOperationException($"Can't access AssemblyDefinition for [{Name}] after it has been loaded!");
        }
    }
}