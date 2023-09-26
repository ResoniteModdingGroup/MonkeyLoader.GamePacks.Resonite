using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader
{
    internal static class MonoCecilExtensions
    {
        public static IEnumerable<TypeDefinition> GetTypes(this AssemblyDefinition assemblyDefinition)
        {
            foreach (var module in assemblyDefinition.Modules)
            {
                foreach (var type in module.Types)
                    yield return type;
            }
        }
    }
}