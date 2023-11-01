using MonkeyLoader.Logging;
using MonkeyLoader.NuGet;
using MonkeyLoader.Patching;
using System;
using System.Runtime.InteropServices;

namespace MonkeyLoader.ConsoleTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(".NET Runtime Version: {0}", Environment.Version.ToString());
            Console.WriteLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"Domain Target Framework: {AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName}");
            Console.WriteLine($"NuGetFramework Name: {NuGetHelper.FrameworkName}");
            Console.WriteLine($"NuGetFramework: {NuGetHelper.Framework}");

            Console.WriteLine("Compatible NuGetFrameworks:");
            foreach (var framework in NuGetHelper.CompatibleFrameworks)
                Console.WriteLine($"{framework} - {framework.GetShortFolderName()}");

            //var attributes = typeof(Program).GetCustomAttributes(false);

            //foreach (var attribute in attributes)
            //{
            //    Console.WriteLine($"Found Attribute with type: {attribute.GetType()}");

            //    if (attribute is FeaturePatchAttribute<TestFeature> patchAttribute)
            //    {
            //        var feature = patchAttribute.GetFeature();
            //        Console.WriteLine("Feature name: " + feature.Name);
            //        Console.WriteLine("Feature description: " + feature.Description);
            //    }
            //}

            var loader = new MonkeyLoader();
            loader.LoggingLevel = LoggingLevel.Trace;
            loader.LoggingHandler = new ConsoleLoggingHandler();

            loader.FullLoad();

            loader.Shutdown();
        }
    }
}