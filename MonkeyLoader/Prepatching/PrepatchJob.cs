using MonkeyLoader.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Prepatching
{
    public class PrepatchJob
    {
        private readonly HashSet<string> typeNames;

        public AssemblyName AssemblyName { get; }

        public EarlyMonkey Prepatcher { get; }

        public IEnumerable<string> TypeNames
        {
            get
            {
                foreach (var typeName in typeNames)
                    yield return typeName;
            }
        }

        public PrepatchJob(EarlyMonkey prepatcher, AssemblyName assemblyName, IEnumerable<string> typeNames)
        {
            Prepatcher = prepatcher;
            AssemblyName = assemblyName;
            this.typeNames = new(typeNames);
        }

        internal bool Apply(AssemblyPool assemblyPool, MonkeyLogger logger)
        {
            logger.Info(() => $"Applying pre-patcher [{Prepatcher.Name}] to assembly [{AssemblyName}].");

            // Not doing anything from prepare is success
            if (!Prepatcher.Prepare(AssemblyName))
                return true;

            var changes = false;
            var assembly = assemblyPool[AssemblyName];

            // Use a method on assembly pool for locking?
            lock (assembly)
            {
                assemblyPool.Snapshot(AssemblyName);

                try
                {
                    var modifiedAssembly = assembly;
                    changes |= Prepatcher.PatchAssembly(ref modifiedAssembly, AssemblyName);

                    foreach (var typeDef in modifiedAssembly.GetTypes().Where(typeDef => typeNames.Contains(typeDef.FullName)))
                    {
                        logger.Debug(() => $"Applying pre-patcher [{Prepatcher.Name}] to type [{Path.GetFileNameWithoutExtension(AssemblyName)}::{typeDef.FullName}].");

                        changes |= Prepatcher.PatchType(typeDef, AssemblyName);
                    }

                    var success = Prepatcher.Cleanup(AssemblyName);

                    if (!success || !changes)
                    {
                        logger.Debug(() => $"Pre-patcher [{Prepatcher.Name}] failed {(success && !changes ? "to change anything" : "")} on assembly [{AssemblyName}]. Restoring.");

                        assemblyPool.Restore(AssemblyName);
                        return success;
                    }

                    if (assembly != modifiedAssembly)
                        assemblyPool[AssemblyName] = modifiedAssembly;

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error(() => $"Pre-patcher [{Prepatcher.Name}] threw an exception on assembly [{AssemblyName}].{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}");

                    return false;
                }
            }
        }
    }
}