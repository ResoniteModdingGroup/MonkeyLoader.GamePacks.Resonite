using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

internal sealed class MonkeyLoaderAssemblyLoadContext(
        string resonitePath,
        string monkeyLoaderPath,
        MonkeyLoaderAssemblyLoadContext.AssemblyResolveEventHandler handler)
    : AssemblyLoadContext("MonkeyLoader")
{
    private readonly AssemblyResolveEventHandler? _assemblyResolveEventHandler = handler;
    private readonly AssemblyDependencyResolver _dependencyResolver = new(resonitePath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Debug.WriteLine($"MonkeyLoaderAssemblyLoadContext: Resolving {assemblyName.FullName}");

        if (_assemblyResolveEventHandler != null)
        {
            var resolvedAssembly = _assemblyResolveEventHandler(assemblyName);
            if (resolvedAssembly != null)
            {
                Debug.WriteLine($"=> Resolved assembly: {resolvedAssembly.FullName}");
                return resolvedAssembly;
            }
        }

        var name = assemblyName.Name;
        var mlPath = Path.Combine(monkeyLoaderPath, $"{name}.dll");
        if (File.Exists(mlPath))
            return LoadFromAssemblyPath(mlPath);

        if (_dependencyResolver.ResolveAssemblyToPath(assemblyName) is null)
            return null;

        return LoadFromAssemblyPath(mlPath);
    }

    // TODO: Should not be necessary anymore with the hookfxr changes.
    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        Debug.WriteLine($"MonkeyLoaderAssemblyLoadContext: Resolving unmanaged {unmanagedDllName}");

        if (_dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) is not string resolvedPath)
            return nint.Zero;

        return LoadUnmanagedDllFromPath(resolvedPath);
    }

    public delegate Assembly? AssemblyResolveEventHandler(AssemblyName assemblyName);
}

internal class Program
{
    private static readonly FileInfo _monkeyLoaderPath = new(Path.Combine("MonkeyLoader", "MonkeyLoader.dll"));

    private static readonly FileInfo _resonitePath = new("Renderite.Host.dll");

    private static object? _monkeyLoaderInstance = null;
    private static MethodInfo? _monkeyLoaderResolveAssemblyMethod = null;

    private static async Task Main(string[] args)
    {
        var loadContext = new MonkeyLoaderAssemblyLoadContext(_resonitePath.FullName!, _monkeyLoaderPath.DirectoryName!, (assemblyName) =>
        {
            if (_monkeyLoaderInstance == null || _monkeyLoaderResolveAssemblyMethod == null)
                return null;

            // Attempt to resolve the assembly using MonkeyLoader's method
            var resolvedAssembly = _monkeyLoaderResolveAssemblyMethod.Invoke(_monkeyLoaderInstance, [assemblyName]);
            if (resolvedAssembly is Assembly assembly)
            {
                Debug.WriteLine("=> Resolved assembly: " + assembly.FullName);
                return assembly;
            }

            return null;
        });
        loadContext.Resolving += (context, assembly)
            => throw new Exception("This should never happen, we need to know about all assemblies ahead of time through ML");

        // https://github.com/dotnet/runtime/blob/main/docs/design/features/AssemblyLoadContext.ContextualReflection.md
        using var contextualReflection = loadContext.EnterContextualReflection();

        var monkeyLoaderAssembly = loadContext.LoadFromAssemblyPath(_monkeyLoaderPath.FullName);

        // this is a hack
        var systemManagementPath = RuntimeInformation.RuntimeIdentifier.StartsWith("win")
            ? new FileInfo(Path.Combine("runtimes", "win", "lib", "net10.0", "System.Management.dll"))
            : new FileInfo("System.Management.dll");

        if (systemManagementPath.Exists)
            loadContext.LoadFromAssemblyPath(systemManagementPath.FullName);

        var monkeyLoaderType = monkeyLoaderAssembly.GetType("MonkeyLoader.MonkeyLoader");
        var loggingLevelType = monkeyLoaderAssembly.GetType("MonkeyLoader.Logging.LoggingLevel");
        var traceLogLevel = Enum.Parse(loggingLevelType!, "Trace");

        _monkeyLoaderInstance = Activator.CreateInstance(monkeyLoaderType!, traceLogLevel, "MonkeyLoader/MonkeyLoader.json");
        _monkeyLoaderResolveAssemblyMethod = monkeyLoaderType!.GetMethod("ResolveAssemblyFromPoolsAndMods", BindingFlags.Public | BindingFlags.Instance);
        var fullLoadMethod = monkeyLoaderType!.GetMethod("FullLoad", BindingFlags.Public | BindingFlags.Instance);
        fullLoadMethod!.Invoke(_monkeyLoaderInstance!, null);

        var resoniteAssembly = loadContext.LoadFromAssemblyPath(_resonitePath.FullName);

        var mainResult = resoniteAssembly.EntryPoint!.Invoke(null, [args]);

        if (mainResult is Task task)
            await task;
    }
}