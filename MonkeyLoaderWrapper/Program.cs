using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

internal class Program
{
    private static readonly FileInfo _monkeyLoaderPath = new(Path.Combine("MonkeyLoader", "MonkeyLoader.dll"));

    private static readonly FileInfo _resonitePath = new("Resonite.dll");
    
    private static object? _monkeyLoaderInstance = null;
    private static MethodInfo? _monkeyLoaderResolveAssemblyMethod = null;
    
    private static async Task Main(string[] args)
    {
        var loadContext = new MonkeyLoaderAssemblyLoadContext(_monkeyLoaderPath.DirectoryName!, (assemblyName) =>
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
        
        var monkeyLoaderAssembly = loadContext.LoadFromAssemblyPath(_monkeyLoaderPath.FullName);
        
        var monkeyLoaderType = monkeyLoaderAssembly.GetType("MonkeyLoader.MonkeyLoader");
        var loggingLevelType = monkeyLoaderAssembly.GetType("MonkeyLoader.Logging.LoggingLevel");
        var traceLogLevel = Enum.Parse(loggingLevelType!, "Trace");

        _monkeyLoaderInstance = Activator.CreateInstance(monkeyLoaderType!, traceLogLevel, "MonkeyLoader/MonkeyLoader.json");
        _monkeyLoaderResolveAssemblyMethod = monkeyLoaderType!.GetMethod("ResolveAssemblyFromPoolsAndMods", BindingFlags.Public | BindingFlags.Instance);
        var fullLoadMethod = monkeyLoaderType!.GetMethod("FullLoad", BindingFlags.Public | BindingFlags.Instance); 
        fullLoadMethod!.Invoke(_monkeyLoaderInstance!, null);

        var resoniteAssembly = loadContext.LoadFromAssemblyPath(_resonitePath.FullName);

        // TODO: Should not be necessary anymore with the hookfxr changes. Either way, should be done by the load context
        NativeLibrary.SetDllImportResolver(resoniteAssembly, ResolveNativeLibrary);
        NativeLibrary.SetDllImportResolver(AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName!.Contains("SteamAudio.NET")), ResolveNativeLibrary);

        var mainResult = resoniteAssembly.EntryPoint!.Invoke(null, [args]);

        if (mainResult is Task task)
            await task;
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var runtimesPath = Path.Combine(_resonitePath.DirectoryName!, "runtimes",
            RuntimeInformation.RuntimeIdentifier, "native");

        var libraryPath = Path.Combine(runtimesPath, $"{libraryName}.dll");

        if (File.Exists(libraryPath))
            return NativeLibrary.Load(libraryPath);

        return IntPtr.Zero;
    }
}

internal class MonkeyLoaderAssemblyLoadContext(
    string monkeyLoaderPath,
    MonkeyLoaderAssemblyLoadContext.AssemblyResolveEventHandler handler)
    : AssemblyLoadContext("MonkeyLoader")
{
    public delegate Assembly? AssemblyResolveEventHandler(AssemblyName assemblyName);
    
    private readonly AssemblyResolveEventHandler? _assemblyResolveEventHandler = handler;

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
        return File.Exists(mlPath) ? LoadFromAssemblyPath(mlPath) : null;
    }
}