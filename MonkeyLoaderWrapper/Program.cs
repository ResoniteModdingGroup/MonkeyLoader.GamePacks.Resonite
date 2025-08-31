﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

internal class MonkeyLoaderAssemblyLoadContext(
    string monkeyLoaderPath,
    MonkeyLoaderAssemblyLoadContext.AssemblyResolveEventHandler handler)
    : AssemblyLoadContext("MonkeyLoader")
{
    private readonly AssemblyResolveEventHandler? _assemblyResolveEventHandler = handler;

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        Debug.WriteLine($"MonkeyLoaderAssemblyLoadContext: Resolving {assemblyName.FullName}");

        if (assemblyName.Name == "0Harmony")
        {
            return LoadFromAssemblyPath(Path.Combine("BepInEx", "core", "0Harmony.dll"));
        }

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

    public delegate Assembly? AssemblyResolveEventHandler(AssemblyName assemblyName);
}

internal class Program
{
    private static readonly FileInfo _monkeyLoaderPath = new(Path.Combine("MonkeyLoader", "MonkeyLoader.dll"));

    private static readonly FileInfo _bepisPath = new(Path.Combine("BepInEx", "core", "BepInEx.NET.CoreCLR.dll"));

    private static readonly FileInfo _resonitePath = new("Renderite.Host.dll");

    private static object? _monkeyLoaderInstance = null;
    private static MethodInfo? _monkeyLoaderResolveAssemblyMethod = null;

    internal static string resoDir = string.Empty;
    internal static AssemblyLoadContext alc = null!;

    static void BepisMain(string[] args, AssemblyLoadContext monkeyLoadContext, string resoPath)
    {
        resoDir = Path.GetDirectoryName(resoPath)!;

        alc = monkeyLoadContext;

        var bepinPath = Path.Combine(resoDir, "BepInEx");
        var bepinArg = Array.IndexOf(args.Select(x => x?.ToLowerInvariant()).ToArray(), "--bepinex-target");
        if (bepinArg != -1 && args.Length > bepinArg + 1)
        {
            bepinPath = args[bepinArg + 1];
        }

        var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "BepInEx.NET.CoreCLR");
        //var asm = alc.LoadFromAssemblyPath(Path.Combine(bepinPath, "core", "BepInEx.NET.CoreCLR.dll"));

        var t = asm.GetType("StartupHook");
        var m = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, [typeof(string), typeof(string), typeof(AssemblyLoadContext)]);
        m.Invoke(null, [resoPath, bepinPath, alc]);
    }

    private static IEnumerable<string> LibraryExtensions
    {
        get
        {
            yield return ".dll";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                yield break;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                yield return ".so";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                yield return ".dylib";
        }
    }

    private static IEnumerable<string> LibraryPrefixes
    {
        get
        {
            yield return string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                yield break;

            yield return "lib";
        }
    }

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

        // https://github.com/dotnet/runtime/blob/main/docs/design/features/AssemblyLoadContext.ContextualReflection.md
        using var contextualReflection = loadContext.EnterContextualReflection();

        foreach (var file in Directory.GetFiles(_bepisPath.DirectoryName!).Where(f => f.EndsWith(".dll")))
        {
            loadContext.LoadFromAssemblyPath(file);
        }
        
        var monkeyLoaderAssembly = loadContext.LoadFromAssemblyPath(_monkeyLoaderPath.FullName);

        // this is a hack
        var systemManagementPath = RuntimeInformation.RuntimeIdentifier.StartsWith("win")
            ? new FileInfo(Path.Combine("runtimes", "win", "lib", "net9.0", "System.Management.dll"))
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

        BepisMain(args, loadContext, _resonitePath.FullName);

        // TODO: Should not be necessary anymore with the hookfxr changes. Either way, should be done by the load context
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            NativeLibrary.SetDllImportResolver(assembly, ResolveNativeLibrary);

        // Find and load Resonite
        var resoAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Renderite.Host");
        if (resoAsm == null)
        {
            resoAsm = alc.LoadFromAssemblyPath(_resonitePath.FullName);
        }

        try
        {
            var result = resoAsm.EntryPoint!.Invoke(null, [args]);
            if (result is Task task) task.Wait();
        }
        catch (Exception e)
        {
            File.WriteAllLines("BepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Resonite crashed", e.ToString()]);
        }

        
    }

    private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == "rnnoise")
            return IntPtr.Zero;

        var runtimesPath = Path.Combine(_resonitePath.DirectoryName!, "runtimes",
            RuntimeInformation.RuntimeIdentifier, "native");

        foreach (var libraryPrefix in LibraryPrefixes)
        {
            foreach (var libraryExtension in LibraryExtensions)
            {
                var libraryPath = Path.Combine(runtimesPath, $"{libraryPrefix}{libraryName}{libraryExtension}");

                if (File.Exists(libraryPath))
                    return NativeLibrary.Load(libraryPath);
            }
        }

        return IntPtr.Zero;
    }
}