using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class TargetTypeAttribute : MonkeyLoaderAttribute
    {
        public string AssemblyName { get; }

        public string FullTypeName { get; }

        public TargetTypeAttribute(string assemblyName, string fullTypeName)
        {
            AssemblyName = assemblyName;
            FullTypeName = fullTypeName;
        }
    }
}