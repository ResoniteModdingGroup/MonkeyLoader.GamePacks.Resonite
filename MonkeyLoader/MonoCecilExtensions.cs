using MonkeyLoader.NuGet;
using Mono.Cecil;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    internal static class MonoCecilExtensions
    {
        public static IEnumerable<AssemblyNameReference> GetAssemblyReferences(this AssemblyDefinition assemblyDefinition)
            => assemblyDefinition.Modules.SelectMany(module => module.AssemblyReferences);

        public static NuGetFramework GetTargetFramework(this AssemblyDefinition assemblyDefinition)
        {
            var customAttribute = assemblyDefinition.CustomAttributes
                .Where(attribute => attribute.AttributeType.Name == nameof(TargetFrameworkAttribute))
                .FirstOrDefault();

            var customAttributeValue = customAttribute?.ConstructorArguments.FirstOrDefault().Value.ToString();

            if (string.IsNullOrWhiteSpace(customAttributeValue))
                return NuGetHelper.Framework;

            try
            {
                return NuGetFramework.Parse(customAttributeValue!);
            }
            catch
            {
                return NuGetHelper.Framework;
            }
        }

        public static IEnumerable<TypeDefinition> GetTypes(this AssemblyDefinition assemblyDefinition)
        {
            foreach (var module in assemblyDefinition.Modules)
            {
                foreach (var type in module.Types)
                    yield return type;
            }
        }

        public static IEnumerable<PackageDependency> ToPackageDependencies(this IEnumerable<AssemblyNameReference> assemblyReferences)
        {
            foreach (var assemblyReference in assemblyReferences)
            {
                yield return new PackageDependency(assemblyReference.Name,
                    new VersionRange(new NuGetVersion(assemblyReference.Version), new FloatRange(NuGetVersionFloatBehavior.PrereleaseMajor)));
            }
        }
    }
}