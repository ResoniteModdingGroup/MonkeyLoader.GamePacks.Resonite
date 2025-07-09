using System.Reflection;
using System.Runtime.InteropServices;

var monkeyLoaderPath = new FileInfo(Path.Combine("MonkeyLoader", "MonkeyLoader.dll")); 
var resonitePath = new FileInfo("Resonite.dll");

// This ONLY applies to ML assemblies, so what is required to run ML. We don't want to load anything else with this handler.
AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
    var name = args.Name.Split(',')[0];
    
    var mlPath = Path.Combine(monkeyLoaderPath.DirectoryName!, $"{name}.dll");
    if (File.Exists(mlPath))
    {
        return Assembly.LoadFile(mlPath);
    }
    
    return null;
};

var monkeyLoaderAssembly = Assembly.LoadFile(monkeyLoaderPath.FullName);
var monkeyLoaderType = monkeyLoaderAssembly.GetType("MonkeyLoader.MonkeyLoader"); var types = monkeyLoaderAssembly.GetTypes();
var loggingLevelType = monkeyLoaderAssembly.GetType("MonkeyLoader.Logging.LoggingLevel");
var traceLogLevel = Enum.Parse(loggingLevelType!, "Trace");

var monkeyLoaderInstance = Activator.CreateInstance(monkeyLoaderType!, traceLogLevel, "MonkeyLoader/MonkeyLoader.json");
var fullLoadMethod = monkeyLoaderType!.GetMethod("FullLoad", BindingFlags.Public | BindingFlags.Instance); 
fullLoadMethod!.Invoke(monkeyLoaderInstance!, null);

var resoniteAssembly = Assembly.LoadFile(resonitePath.FullName);

// If we had the game and ML in a load context, this would not be necessary since it can resolve native libraries with a generic callback
NativeLibrary.SetDllImportResolver(resoniteAssembly, ResolveNativeLibrary);
NativeLibrary.SetDllImportResolver(AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName!.Contains("SteamAudio.NET")), ResolveNativeLibrary);

var resoniteProgramType = resoniteAssembly.GetType("Program");
var resoniteMainMethod = resoniteProgramType!.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).First();

var mainResult = resoniteMainMethod!.Invoke(null, [args]);

if (mainResult is Task task)
{
    await task;
}

return;

IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
{
    var runtimesPath = Path.Combine(resonitePath.DirectoryName!, "runtimes",
        RuntimeInformation.RuntimeIdentifier, "native");
    
    var libraryPath = Path.Combine(runtimesPath, $"{libraryName}.dll");
    
    if (File.Exists(libraryPath))
    {
        return NativeLibrary.Load(libraryPath);
    }
    
    return IntPtr.Zero;
}