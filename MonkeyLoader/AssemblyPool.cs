using MonkeyLoader.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed class AssemblyPool : IAssemblyResolver
    {
        private readonly Dictionary<AssemblyName, AssemblyEntry> _assemblies = new();
        private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<AssemblyPool> _fallbackPools = new();
        private readonly Func<string?>? _getPatchedAssemblyPath;
        private readonly MonkeyLogger _logger;

        /// <summary>
        /// Gets whether assemblies will be loaded when asked to resolve them.
        /// </summary>
        public bool LoadForResolve { get; }

        public string? PatchedAssemblyPath => _getPatchedAssemblyPath?.Invoke();

        /// <summary>
        /// Creates a new <see cref="AssemblyPool"/> instance, loading assemblies when asked to resolve them if desired.
        /// </summary>
        /// <param name="getPatchedAssemblyPath">Provides the path where to save patched assemblies. Return <c>null</c> to disable.</param>
        /// <param name="loadForResolve">Whether to load assemblies when asked to resolve them.</param>
        public AssemblyPool(MonkeyLogger logger, Func<string?>? getPatchedAssemblyPath = null, bool loadForResolve = true)
        {
            _logger = logger;
            _getPatchedAssemblyPath = getPatchedAssemblyPath;
            LoadForResolve = loadForResolve;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public bool AddFallbackPool(AssemblyPool pool) => _fallbackPools.Add(pool);

        public void AddSearchDirectory(string directory)
        {
            _directories.Add(directory);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> for an entry without attempting to load it.
        /// </summary>
        /// <param name="name">The name of the <see cref="Assembly"/> to get.</param>
        /// <returns>The loaded <see cref="Assembly"/> if it was loaded already.</returns>
        /// <exception cref="InvalidOperationException">When the assembly hasn't been loaded yet.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public Assembly GetAssembly(AssemblyName name) => GetEntry(name).GetAssembly();

        /// <summary>
        /// Loads all (not yet loaded) <see cref="Assembly"/> entries.
        /// </summary>
        public void LoadAll(string path)
        {
            var sw = Stopwatch.StartNew();

            Directory.CreateDirectory(path);

            //var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => new AssemblyName(assembly.GetName().Name)).ToHashSet();

            //var entries = _assemblies.Values
            //    .Where(entry => !entry.Loaded)
            //    .TopologicalSort(entry => entry.Name, entry => entry.GetDependencies(alreadyLoaded))
            //    .ToArray();

            //foreach (var entry in entries)
            //    entry.LoadAssembly(_logger, PatchedAssemblyPath);

            foreach (var assemblyPath in _assemblies.Values.Select(entry => entry.SaveAssembly(path, _logger)).Where(path => path is not null).ToArray())
                Assembly.LoadFile(assemblyPath);

            _logger.Info(() => $"Loaded all {_assemblies.Count} assembly definitions in {sw.ElapsedMilliseconds}ms!");
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> for an entry loading it if necessary.
        /// </summary>
        /// <param name="name">The name of the <see cref="Assembly"/> to get.</param>
        /// <returns>The loaded <see cref="Assembly"/>.</returns>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public Assembly LoadAssembly(AssemblyName name) => GetEntry(name).LoadAssembly(_logger, PatchedAssemblyPath);

        public AssemblyDefinition LoadDefinition(string path, ReaderParameters? readerParameters = null)
        {
            var name = new AssemblyName(path, true);
            if (TryResolve(name, out var assemblyDefinition))
                return assemblyDefinition;

            readerParameters ??= new ReaderParameters() { AssemblyResolver = this };
            var definition = AssemblyDefinition.ReadAssembly(path, readerParameters);
            name = new AssemblyName(definition.Name.Name);

            if (TryResolve(name, out assemblyDefinition))
                return assemblyDefinition;

            _assemblies.Add(name, new AssemblyEntry(name, definition));
            _logger.Debug(() => $"Loaded assembly definition from {path}");

            return definition;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
            => Resolve(name, new ReaderParameters() { AssemblyResolver = this });

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var entryName = new AssemblyName(name.Name);

            if (TryResolve(entryName, out var assemblyDefinition))
                return assemblyDefinition;

            foreach (var fallbackPool in _fallbackPools)
            {
                if (fallbackPool.TryResolve(entryName, out assemblyDefinition))
                    return assemblyDefinition;
            }

            var assembly = SearchDirectory(name, parameters);
            if (assembly is not null)
                return assembly;

            throw new AssemblyResolutionException(name);
        }

        /// <summary>
        /// Restores the <see cref="AssemblyDefinition"/> for an entry and releases its definition lock.
        /// </summary>
        /// <param name="name">The entry to restore.</param>
        /// <exception cref="InvalidOperationException">When the entry has already been loaded or returned.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public void RestoreDefinition(AssemblyName name) => GetEntry(name).RestoreDefinition();

        /// <summary>
        /// Sets the given (new) <see cref="AssemblyDefinition"/> for and entry and releases its definition lock.
        /// </summary>
        /// <param name="name">The entry to set.</param>
        /// <param name="assemblyDefinition">The (new) <see cref="AssemblyDefinition"/>.</param>
        /// <exception cref="InvalidOperationException">When the entry has already been loaded or returned.</exception>
        /// <exception cref="KeyNotFoundException">When the <paramref name="name"/> doesn't exist in this pool.</exception>
        public void ReturnDefinition(AssemblyName name, AssemblyDefinition assemblyDefinition)
            => GetEntry(name).ReturnDefinition(assemblyDefinition);

        public bool TryResolve(AssemblyName name, [NotNullWhen(true)] out AssemblyDefinition? assemblyDefinition)
        {
            if (TryGetEntry(name, out var entry))
            {
                assemblyDefinition = entry.GetResolveDefinition();
                return true;
            }

            assemblyDefinition = null;
            return false;
        }

        /// <summary>
        /// Tries to wait until nothing else is modifying the <see cref="AssemblyDefinition"/> of an entry anymore,
        /// before making a snapshot and returning it. The definition has to be returned using
        /// <see cref="RestoreDefinition"/> or <see cref="ReturnDefinition"/> exactly once.
        /// </summary>
        /// <param name="name">The entry to get.</param>
        /// <param name="result">The entry's <see cref="AssemblyDefinition"/> or <c>null</c> if it wasn't found or has already been loaded.</param>
        /// <returns>Whether the definition was returned.</returns>
        public bool TryWaitForDefinition(AssemblyName name, [NotNullWhen(true)] out AssemblyDefinition? result)
        {
            if (!TryGetEntry(name, out var entry) || entry.Loaded)
            {
                result = null;
                return false;
            }

            result = entry.WaitForDefinition();
            return true;
        }

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
            => GetEntry(name).WaitForDefinition();

        private AssemblyEntry GetEntry(AssemblyName name)
        {
            if (!_assemblies.TryGetValue(name, out var assemblyDefinition))
                throw new KeyNotFoundException($"No AssemblyDefinition loaded for [{name}]!");

            return assemblyDefinition;
        }

        private Assembly? ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(AssemblyNameReference.Parse(args.Name).Name);

            if (!_assemblies.TryGetValue(name, out var entry))
                return null;

            if (LoadForResolve)
                return entry.LoadAssembly(_logger, PatchedAssemblyPath);

            return entry.GetAssembly();
        }

        private AssemblyDefinition? SearchDirectory(AssemblyNameReference name, ReaderParameters parameters)
        {
            parameters.AssemblyResolver ??= this;
            var extensions = name.IsWindowsRuntime ? new[] { ".winmd", ".dll" } : new[] { ".exe", ".dll" };

            foreach (var directory in _directories)
            {
                foreach (var extension in extensions)
                {
                    var file = Path.Combine(directory, name.Name + extension);

                    if (!File.Exists(file))
                        continue;

                    try
                    {
                        return LoadDefinition(file, parameters);
                    }
                    catch (BadImageFormatException)
                    { }
                }
            }

            return null;
        }

        private bool TryGetEntry(AssemblyName name, out AssemblyEntry entry)
            => _assemblies.TryGetValue(name, out entry);

        private sealed class AssemblyEntry : IDisposable
        {
            public readonly AssemblyName Name;
            private bool _changes = false;
            private AssemblyDefinition _definition;
            private AutoResetEvent? _definitionLock;
            private MemoryStream? _definitionSnapshot;
            private bool _disposedValue = false;
            private Assembly? _loadedAssembly;

            [MemberNotNullWhen(true, nameof(_loadedAssembly))]
            [MemberNotNullWhen(false, nameof(_definition), nameof(_definitionLock), nameof(_definitionSnapshot))]
            public bool Loaded => _loadedAssembly != null;

            public AssemblyEntry(AssemblyName name, AssemblyDefinition definition)
            {
                Name = name;
                _definition = definition;
                _definitionLock = new(true);
                _definitionSnapshot = new();
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public Assembly GetAssembly()
            {
                if (!Loaded)
                    throw new InvalidOperationException($"Assembly for [{Name}] hasn't been loaded yet!");

                return _loadedAssembly;
            }

            public IEnumerable<AssemblyName> GetDependencies(HashSet<AssemblyName> alreadyLoaded)
            {
                var fullNames = Loaded ?
                    _loadedAssembly.GetReferencedAssemblies().Select(assembly => assembly.Name)
                    : _definition.Modules.SelectMany(module => module.AssemblyReferences)
                        .Select(reference => AssemblyNameReference.Parse(reference.Name).Name);

                return fullNames.Select(name => new AssemblyName(name))
                    .Where(name => !alreadyLoaded.Contains(name));
            }

            public AssemblyDefinition GetResolveDefinition() => _definition;

            public Assembly LoadAssembly(MonkeyLogger logger, string? patchedAssemblyPath)
            {
                var saveAssemblies = true;
                if (string.IsNullOrWhiteSpace(patchedAssemblyPath) || !Directory.Exists(patchedAssemblyPath))
                {
                    saveAssemblies = false;
                    logger.Debug(() => $"Save path for patched assemblies wasn't found: {patchedAssemblyPath}");
                }

                if (!Loaded)
                {
                    WaitForDefinition();
                    var definitionBytes = _definitionSnapshot.ToArray();

                    if (saveAssemblies && _changes)
                    {
                        var targetPath = Path.Combine(patchedAssemblyPath, $"{Name}.dll");

                        try
                        {
                            File.WriteAllBytes(targetPath, definitionBytes);
                            logger.Trace(() => $"Saved patched assembly to {targetPath}");
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(() => ex.Format($"Exception while trying to save assembly to {targetPath}"));
                        }
                    }

                    _loadedAssembly = Assembly.Load(definitionBytes);
                    logger.Trace(() => $"Loaded assembly definition [{Name}]");

                    _definitionSnapshot.Dispose();
                    _definitionSnapshot = null;

                    _definitionLock.Dispose();
                    _definitionLock = null;
                }

                return _loadedAssembly;
            }

            public void RestoreDefinition()
            {
                if (Loaded)
                    ThrowLoadedInvalidOperation();

                ThrowIfAlreadyReturned();

                _definition.Dispose();
                _definitionSnapshot.Position = 0;
                _definition = AssemblyDefinition.ReadAssembly(_definitionSnapshot);

                _definitionLock.Set();
            }

            public void ReturnDefinition(AssemblyDefinition assemblyDefinition)
            {
                if (Loaded)
                    ThrowLoadedInvalidOperation();

                ThrowIfAlreadyReturned();

                _definition = assemblyDefinition;
                _definitionLock.Set();
                _changes = true;
            }

            public AssemblyDefinition WaitForDefinition()
            {
                if (Loaded)
                    ThrowLoadedInvalidOperation();

                _definitionLock.WaitOne();

                _definitionSnapshot.SetLength(0);
                _definition.Write(_definitionSnapshot);

                return _definition;
            }

            internal string? SaveAssembly(string path, MonkeyLogger logger)
            {
                WaitForDefinition();
                var definitionBytes = _definitionSnapshot!.ToArray();

                var targetPath = Path.Combine(path, $"{Name}.dll");

                try
                {
                    File.WriteAllBytes(targetPath, definitionBytes);
                    logger.Trace(() => $"Saved patched assembly to {targetPath}");
                    return targetPath;
                }
                catch (Exception ex)
                {
                    logger.Warn(() => ex.Format($"Exception while trying to save assembly to {targetPath}"));
                }

                return null;
            }

            private void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing && !Loaded)
                    {
                        _definition.Dispose();
                        _definitionSnapshot.Dispose();
                        _definitionLock.Dispose();
                    }

                    _disposedValue = true;
                }
            }

            private void ThrowIfAlreadyReturned()
            {
                if (_definitionLock!.WaitOne(0))
                {
                    _definitionLock.Set();
                    throw new InvalidOperationException($"Can't return or restore AssemblyDefinition for [{Name}] when it wasn't taken first (using {nameof(WaitForDefinition)})!");
                }
            }

            [DoesNotReturn]
            private void ThrowLoadedInvalidOperation()
                => throw new InvalidOperationException($"Can't access AssemblyDefinition for [{Name}] after it has been loaded!");
        }
    }
}