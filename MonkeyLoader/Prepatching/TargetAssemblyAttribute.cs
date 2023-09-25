using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class TargetAssemblyAttribute : MonkeyLoaderAttribute
    {
        public string AssemblyName { get; }

        public TargetAssemblyAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}