using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyLoader.Resonite
{
    internal sealed class Publiciser : EarlyMonkey<Publiciser>
    {
        public override string Name { get; } = nameof(Publiciser);
        public override IEnumerable<PrePatchTarget> PrePatchTargets => PubliciserSettings.Assemblies.Select(assembly => new PrePatchTarget(new AssemblyName(assembly)));

        public override bool Run()
        {
            Config.LoadSection<PubliciserSettings>();
            return base.Run();
        }

        protected override bool Patch(PatchJob patchJob)
        {
            foreach (var module in patchJob.Assembly.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    type.IsPublic = true;

                    if (type.IsNested)
                        type.IsNestedPublic = true;

                    foreach (var field in type.Fields)
                    {
                        if (!type.Properties.Any(property => property.Name == field.Name) && !type.Events.Any(e => e.Name == field.Name))
                            field.IsPublic = true;
                    }

                    foreach (var method in type.Methods)
                        method.IsPublic = true;
                }
            }

            patchJob.Changes = true;
            return true;
        }
    }
}