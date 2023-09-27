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

            context.

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
                    .Select(field => field.Type.TryGetBaseType(configKeyType!))
                    .Where(f => f is not null))
                {
                    builder.Append($"        {(configKeyField.DeclaredAccessibility switch { Accessibility.Public => "public", Accessibility.Internal => "internal", _ => "private" })} ");
                    builder.AppendLine($"{configKeyField.");
                }

                //derivedClassDef.GetMembers().Where(symbol => symbol is IFieldSymbol fieldSymbol && fieldSymbol.Type.).Cast<IFieldSymbol>()
            }
            }

            public void Initialize(GeneratorInitializationContext context)
            {
                context.RegisterForSyntaxNotifications(() => configsReceiver);
            }
        }
    }