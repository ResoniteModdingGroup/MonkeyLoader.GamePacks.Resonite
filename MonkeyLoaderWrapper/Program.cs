using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

internal class MonkeyLoaderAssemblyLoadContext(
    string monkeyLoaderPath,
    MonkeyLoaderAssemblyLoadContext.AssemblyResolveEventHandler handler)
    : AssemblyLoadContext("MonkeyLoader")
{
    private readonly AssemblyResolveEventHandler? _assemblyResolveEventHandler = handler;

    internal Assembly? MyLoad(object? sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);
        return Load(name);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        try
        {
            Debug.WriteLine($"MonkeyLoaderAssemblyLoadContext: Resolving {assemblyName.FullName}");

            if (assemblyName.Name == "0Harmony")
                return Program.harmAsm;

            var found = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            if (found != null)
            {
                return found;
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
            if (File.Exists(mlPath))
                return LoadFromAssemblyPath(mlPath);

            return null;
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - MonkeyLoaderAssemblyLoadContext crashed", e.ToString()]);
            throw;
        }
    }

    public delegate Assembly? AssemblyResolveEventHandler(AssemblyName assemblyName);
}

internal class BepisLoadContext : AssemblyLoadContext
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return BepisLoader.BepisResolveInternal(assemblyName);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        try
        {
            Debug.WriteLine($"BepisLoadContext: LoadUnmanagedDll {unmanagedDllName}");

            var rid = GetRuntimeIdentifier();

            var nativeLibs = Path.Join(BepisLoader.resoDir, "runtimes", rid, "native");
            IEnumerable<string> potentialPaths = [unmanagedDllName, Path.Combine(nativeLibs, GetUnmanagedLibraryName(unmanagedDllName))];
            if (unmanagedDllName.EndsWith("steam_api64.so")) potentialPaths = ((IEnumerable<string>)["libsteam_api.so"]).Concat(potentialPaths);

            BepisLoader.Log("=> NativeLib " + unmanagedDllName);
            foreach (var path in potentialPaths)
            {
                BepisLoader.Log("  => Testing: " + path);
                if (File.Exists(path))
                {
                    BepisLoader.Log("  => Exists! " + path);
                    var dll = LoadUnmanagedDllFromPath(path);
                    if (dll != IntPtr.Zero)
                    {
                        BepisLoader.Log("  => Loaded! " + path);
                        return dll;
                    }
                }
            }

            return IntPtr.Zero;
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - BepisLoadContext: LoadUnmanagedDll crashed", e.ToString()]);
            throw;
        }
    }


    private static string GetRuntimeIdentifier()
    {
        string os;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            os = "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            os = "osx";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            os = "linux";
        else
            throw new PlatformNotSupportedException();

        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X86 => "-x86",
            Architecture.X64 => "-x64",
            Architecture.Arm64 => "-arm64",
            _ => ""
        };

        return $"{os}{arch}";
    }
    private static string GetUnmanagedLibraryName(string name)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return $"{name}.dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return $"lib{name}.so";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"lib{name}.dylib";

        throw new PlatformNotSupportedException();
    }
}

internal class BepisLoader
{
    internal static readonly FileInfo _bepisPath = new(Path.Combine("BepInEx", "core", "BepInEx.NET.CoreCLR.dll"));
    internal static string resoDir = string.Empty;
    internal static AssemblyLoadContext alc = null!;

    //static void LoadGameAssemblies(string gameRootDir)
    //{
    //    var loadedAsms = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name);
    //    foreach (var file in Directory.GetFiles(gameRootDir).Where(f => f.EndsWith(".dll")))
    //    {
    //        var filename = Path.GetFileNameWithoutExtension(file);
    //        if (!loadedAsms.Contains(filename))
    //        {
    //            try
    //            {
    //                if (filename == "System.Management")
    //                {
    //                    // this is a hack
    //                    var systemManagementPath = RuntimeInformation.RuntimeIdentifier.StartsWith("win")
    //                        ? new FileInfo(Path.Combine("runtimes", "win", "lib", "net9.0", "System.Management.dll"))
    //                        : new FileInfo("System.Management.dll");

    //                    if (systemManagementPath.Exists)
    //                        alc.LoadFromAssemblyPath(systemManagementPath.FullName);
    //                }
    //                else if (filename == "SemanticVersioning")
    //                {
    //                    // don't load
    //                }
    //                else
    //                {
    //                    alc.LoadFromAssemblyPath(file);
    //                }

    //            }
    //            catch (Exception e)
    //            {
    //                Debug.WriteLine("Failed to load assembly: " + filename);
    //            }
    //        }
    //    }
    //}

    static Assembly? BepisResolveGameDll(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        return BepisResolveInternal(assemblyName);
    }

    internal static Assembly? BepisResolveInternal(AssemblyName assemblyName)
    {
        try
        {
            BepisLoader.Log($"Resolving {assemblyName.FullName}");

            if (assemblyName.Name == "0Harmony")
                return Program.harmAsm;

            var found = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            if (found != null)
            {
                return found;
            }

            if (assemblyName.Name == "System.Management")
            {
                var systemManagementPath = RuntimeInformation.RuntimeIdentifier.StartsWith("win")
                    ? new FileInfo(Path.Combine("runtimes", "win", "lib", "net9.0", "System.Management.dll"))
                    : new FileInfo("System.Management.dll");

                if (systemManagementPath.Exists)
                    return alc.LoadFromAssemblyPath(systemManagementPath.FullName);
            }

            var targetPath = Path.Combine(resoDir, assemblyName.Name + ".dll");
            if (File.Exists(targetPath))
            {
                var asm = alc.LoadFromAssemblyPath(targetPath);
                return asm;
            }

            return null;
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - BepisLoader BepisResolveInternal crashed", e.ToString()]);
            throw;
        }
    }

    internal static void Load(string[] args, string resoPath)
    {
        resoDir = Path.GetDirectoryName(resoPath)!;

        

        alc = new BepisLoadContext();

        //LoadGameAssemblies(resoDir);

        var harm = Directory.GetFiles(_bepisPath.DirectoryName!).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == "0Harmony");
        if (harm != null)
            Program.harmAsm = alc.LoadFromAssemblyPath(harm);
        else
            throw new Exception("Could not find Harmony!");

        // TODO: removing this breaks stuff, idk why
        AppDomain.CurrentDomain.AssemblyResolve += BepisLoader.BepisResolveGameDll;

        var bepinPath = Path.Combine(resoDir, "BepInEx");
        var bepinArg = Array.IndexOf(args.Select(x => x?.ToLowerInvariant()).ToArray(), "--bepinex-target");
        if (bepinArg != -1 && args.Length > bepinArg + 1)
        {
            bepinPath = args[bepinArg + 1];
        }

        //var asm = monkeyLoadContext.Assemblies.FirstOrDefault(x => x.GetName().Name == "BepInEx.NET.CoreCLR");
        //var asm = alc.LoadFromAssemblyPath(Path.Combine(bepinPath, "core", "BepInEx.NET.CoreCLR.dll"));
        var asm = alc.LoadFromAssemblyPath(_bepisPath.FullName);

        var t = asm.GetType("StartupHook");
        var m = t.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, [typeof(string), typeof(string), typeof(AssemblyLoadContext)]);
        m.Invoke(null, [resoPath, bepinPath, alc]);
    }

#if DEBUG
    private static object _lock = new object();
#endif
    internal static void Log(string message)
    {
        Debug.WriteLine("BepisLoader: " + message);
#if DEBUG
        lock (_lock)
        {
            File.AppendAllLines("BepisLoader.log", [message]);
        }
#endif
    }
}

internal class MonkeyLoaderLoader
{
    internal static readonly FileInfo _monkeyLoaderPath = new(Path.Combine("MonkeyLoader", "MonkeyLoader.dll"));
    private static object? _monkeyLoaderInstance = null;
    private static MethodInfo? _monkeyLoaderResolveAssemblyMethod = null;

    internal static void Load()
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
        //loadContext.Resolving += (context, assembly)
        //=> throw new Exception("This should never happen, we need to know about all assemblies ahead of time through ML");

        // https://github.com/dotnet/runtime/blob/main/docs/design/features/AssemblyLoadContext.ContextualReflection.md
        //using var contextualReflection = loadContext.EnterContextualReflection();

        AppDomain.CurrentDomain.AssemblyResolve += loadContext.MyLoad;

        var monkeyLoaderAssembly = loadContext.LoadFromAssemblyPath(_monkeyLoaderPath.FullName);

        //if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "System.Management"))
        //{
        //    // this is a hack
        //    var systemManagementPath = RuntimeInformation.RuntimeIdentifier.StartsWith("win")
        //        ? new FileInfo(Path.Combine("runtimes", "win", "lib", "net9.0", "System.Management.dll"))
        //        : new FileInfo("System.Management.dll");

        //    if (systemManagementPath.Exists)
        //        loadContext.LoadFromAssemblyPath(systemManagementPath.FullName);
        //}
        

        var monkeyLoaderType = monkeyLoaderAssembly.GetType("MonkeyLoader.MonkeyLoader");
        var loggingLevelType = monkeyLoaderAssembly.GetType("MonkeyLoader.Logging.LoggingLevel");
        var traceLogLevel = Enum.Parse(loggingLevelType!, "Trace");

        _monkeyLoaderInstance = Activator.CreateInstance(monkeyLoaderType!, traceLogLevel, "MonkeyLoader/MonkeyLoader.json");
        _monkeyLoaderResolveAssemblyMethod = monkeyLoaderType!.GetMethod("ResolveAssemblyFromPoolsAndMods", BindingFlags.Public | BindingFlags.Instance);
        var fullLoadMethod = monkeyLoaderType!.GetMethod("PartialLoad", BindingFlags.NonPublic | BindingFlags.Instance);

        fullLoadMethod!.Invoke(_monkeyLoaderInstance!, null);
    }
}

internal class Program
{
    internal static readonly FileInfo _resonitePath = new("Renderite.Host.dll");
    internal static Assembly harmAsm = null;

    private static async Task Main(string[] args)
    {
        

        try
        {
            BepisLoader.Load(args, _resonitePath.FullName);
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - BepisLoader crashed", e.ToString()]);
            throw;
        }

        try
        {
            MonkeyLoaderLoader.Load();
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - MonkeyLoaderLoader crashed", e.ToString()]);
            throw;
        }

        //AppDomain.CurrentDomain.AssemblyResolve += loadContext.MyLoad;

        //foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        //{
        //    if (asm.GetName().Name == "0Harmony")
        //    {
        //        Debug.WriteLine("Found Harmony: " + asm.Location + "\n" + asm.GetName().FullName);
        //    }
        //    if (asm.GetName().Name == "System.Management")
        //    {
        //        Debug.WriteLine("Found System.Management: " + asm.Location + "\n" + asm.GetName().FullName);
        //    }
        //}

        //Console.ReadLine();

        //foreach (var file in Directory.GetFiles(_bepisPath.DirectoryName!).Where(f => f.EndsWith(".dll")))
        //{
        //    loadContext.LoadFromAssemblyPath(file);
        //}

        //BepisMain(args, loadContext, _resonitePath.FullName);


        // TODO: Should not be necessary anymore with the hookfxr changes. Either way, should be done by the load context
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            NativeLibrary.SetDllImportResolver(assembly, ResolveNativeLibrary);

        // Find and load Resonite
        var resoAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Renderite.Host");
        if (resoAsm == null)
        {
            resoAsm = AssemblyLoadContext.Default.LoadFromAssemblyPath(_resonitePath.FullName);
            //File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Could not find Renderite.Host"]);
            //throw new Exception("Could not find Renderite.Host");
        }
        try
        {
            var result = resoAsm.EntryPoint!.Invoke(null, [args]);
            if (result is Task task) task.Wait();
        }
        catch (Exception e)
        {
            File.WriteAllLines("0MonkeyBepisCrash.log", [DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Resonite crashed", e.ToString()]);
            throw;
        }
        //throw new Exception("BAD");
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