using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Bluehill.Analyzers;

[Generator(LanguageNames.CSharp)]
public sealed class EnumExtensionsGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var values = context.SyntaxProvider.ForAttributeWithMetadataName("Bluehill.EnumExtensionsAttribute",
            static (n, _) => n is EnumDeclarationSyntax,
             (c, _) => ((CSharpCompilation)c.SemanticModel.Compilation).LanguageVersion >= LanguageVersion.CSharp10 ? GetEnumInfo((INamedTypeSymbol)c.TargetSymbol) : (EnumInfo?)null);

        context.RegisterSourceOutput(values, Generate);
    }

    private static void Generate(SourceProductionContext context, EnumInfo? info) {
        if (info is null) {
            return;
        }

        var (fqnName, @namespace, accessibility, isFlags, members) = info.Value;
        string accessibilityKeyword;

        try {
            accessibilityKeyword = GetAccessibilityKeyword(accessibility);
        } catch (ArgumentOutOfRangeException) {
            return;
        }

        var typeName = GetTypeName(fqnName, @namespace);
        var tnArray = typeName.Split('.');
        StringBuilder sb = new();

        sb.Append("using static ").Append(fqnName).AppendLine(";").AppendLine();

        if (!string.IsNullOrWhiteSpace(@namespace)) {
            sb.Append("namespace ").Append(@namespace).AppendLine(";").AppendLine();
        }

        sb.Append(accessibilityKeyword).Append(" static class ").Append(GetEscapedName(typeName)).AppendLine("Extensions {")
            .Append("    ").Append(accessibilityKeyword).Append(" static string ToStringFast(this ").Append(typeName).AppendLine(" value) => value switch {");

        foreach (var member in members) {
            sb.Append("        ");

            if (member == tnArray[tnArray.Length - 1]) {
                sb.Append(typeName).Append('.');
            }

            sb.Append(member).Append(" => nameof(");

            if (member == tnArray[tnArray.Length - 1]) {
                sb.Append(typeName).Append('.');
            }

            sb.Append(member).AppendLine("),");
        }

        sb.AppendLine("        _ => value.ToString()").AppendLine("    };");

        if (isFlags) {
            sb.AppendLine().Append("    ").AppendLine("[System.Runtime.CompilerServices.MethodImplAttribute(256)]").Append("    ").Append(accessibilityKeyword).Append(" static bool HasFlagFast(this ").Append(typeName).Append(" value, ")
                .Append(typeName).AppendLine(" flag) => (value & flag) == flag;");
        }

        sb.AppendLine("}");

        context.AddSource($"{fqnName}Extensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static EnumInfo GetEnumInfo(INamedTypeSymbol enumSymbol)
        => new(enumSymbol.ToDisplayString(FqnFormat), enumSymbol.ContainingNamespace?.MetadataName, GetTypeAccessibility(enumSymbol),
            enumSymbol.HasAttribute(MetadataName.Parse("System.FlagsAttribute")), Array.AsReadOnly(enumSymbol.MemberNames.ToArray()));

    private static string GetAccessibilityKeyword(Accessibility accessibility) => accessibility switch {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        _ => throw new ArgumentOutOfRangeException(nameof(accessibility))
    };
}
