using MonkeyLoader.Game;
using MonkeyLoader.Patching;
using System;

namespace MonkeyLoader.ConsoleTest
{
    [FeaturePatch<TestFeature>((PatchSeverity)0)]
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(".NET Runtime Version: {0}", Environment.Version.ToString());

            var attributes = typeof(Program).GetCustomAttributes(false);

            foreach (var attribute in attributes)
            {
                Console.WriteLine($"Found Attribute with type: {attribute.GetType()}");

                if (attribute is FeaturePatchAttribute<TestFeature> patchAttribute)
                {
                    var feature = patchAttribute.GetFeature();
                    Console.WriteLine("Feature name: " + feature.Name);
                    Console.WriteLine("Feature description: " + feature.Description);
                }
            }
        }
    }
}