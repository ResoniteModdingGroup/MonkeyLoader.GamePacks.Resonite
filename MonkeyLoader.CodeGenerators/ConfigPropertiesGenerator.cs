using Microsoft.CodeAnalysis;
using SourceGeneratorsKit;
using System;
using System.Linq;
using System.Text;

namespace MonkeyLoader.CodeGenerators
{
    [Generator]
    public class ConfigPropertiesGenerator : ISourceGenerator
    {
        private readonly DerivedClassesReceiver configsReceiver = new("ModConfig");

        public void Execute(GeneratorExecutionContext context)
        {
            var builder = new StringBuilder();
            var configKeyType = context.Compilation.GetTypeByMetadataName("MonkeyLoader.Config.ModConfigKey`1");

            foreach (var derivedClassDef in configsReceiver.Classes)
            {
                var filename = derivedClassDef.FullMetadataName();

                builder.AppendLine($"namespace {derivedClassDef.FullNamespace()}");
                builder.AppendLine("{");
                builder.Append($"    {(derivedClassDef.DeclaredAccessibility is Accessibility.NotApplicable or Accessibility.Private ? "internal" : "public")} ");
                builder.AppendLine($" {(derivedClassDef.IsSealed ? "sealed " : "")} partial class {derivedClassDef.Name} : {derivedClassDef.BaseType!.Name}");

                foreach (var configKeyField in derivedClassDef.GetMembers()
                    .Where(member => member is IFieldSymbol field && field.IsStatic)
                    .Cast<IFieldSymbol>()
                    .Select(field => (field, type: (INamedTypeSymbol)field.Type.TryGetBaseType(configKeyType!)!))
                    .Where(f => f.type is not null))
                {
                    builder.Append($"        {(configKeyField.field.DeclaredAccessibility switch { Accessibility.Public => "public", Accessibility.Internal => "internal", _ => "private" })} ");
                    builder.AppendLine($"{configKeyField.type.TypeParameters.First().TryFullName()} {configKeyField.field.Name} {{ get; set; }}");
                }

                builder.AppendLine("    }");
                builder.AppendLine("}");

                context.AddSource(filename, builder.ToString());
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => configsReceiver);
        }
    }
}