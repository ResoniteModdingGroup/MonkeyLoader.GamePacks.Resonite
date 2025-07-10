using HarmonyLib;
using MonkeyLoader.NuGet;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Reflection;
using System.Runtime.InteropServices;

internal class Program
{
    private static readonly FileInfo _monkeyLoaderPath = new(Path.Combine("MonkeyLoader", "MonkeyLoader.dll"));

    private static readonly FileInfo _resonitePath = new("Resonite.dll");

    static Program()
    {
        // This ONLY applies to ML assemblies, so what is required to run ML. We don't want to load anything else with this handler.
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var name = args.Name.Split(',')[0];

            var mlPath = Path.Combine(_monkeyLoaderPath.DirectoryName!, $"{name}.dll");
            if (File.Exists(mlPath))
                return Assembly.LoadFile(mlPath);

            return null;
        };
    }

    private static async Task Main(string[] args)
    {
        var monkeyLoader = new MonkeyLoader.MonkeyLoader();
        monkeyLoader.FullLoad();

        var resoniteAssembly = Assembly.LoadFile(_resonitePath.FullName);

        // If we had the game and ML in a load context, this would not be necessary since it can resolve native libraries with a generic callback
        NativeLibrary.SetDllImportResolver(resoniteAssembly, ResolveNativeLibrary);
        NativeLibrary.SetDllImportResolver(AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName!.Contains("SteamAudio.NET")), ResolveNativeLibrary);

        var resoniteProgramType = resoniteAssembly.GetType("Program");
        var mainResult = resoniteAssembly.EntryPoint!.Invoke(null, [args]);

        if (mainResult is Task task)
            await task;

        return;
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