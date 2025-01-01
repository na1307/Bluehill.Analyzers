using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0002FieldsShouldBeAtTheTopAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = "BH0002";
    private const string category = "Style";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Warning, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0002");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(semanticModelAction);
    }

    private static void semanticModelAction(SemanticModelAnalysisContext context) {
        var token = context.CancellationToken;
        var model = context.SemanticModel;

        foreach (var variable in model.SyntaxTree.GetRoot(token).DescendantNodesAndSelf()
            .OfType<VariableDeclarationSyntax>().SelectMany(v => v.Variables)) {
            // Skip if variable is not a field
            if (model.GetDeclaredSymbol(variable, token) is not IFieldSymbol fieldSymbol) {
                continue;
            }

            var type = fieldSymbol.ContainingType;

            // Skip partial types
            if (type.DeclaringSyntaxReferences.Select(s => s.GetSyntax(token) as TypeDeclarationSyntax)
                .Any(tds => tds?.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) ?? false)) {
                continue;
            }

            // Suppress on record types (I don't know how to handle the record's primary constructor.)
            if (type.GetMembers().Any(m => m.Name == "<Clone>$")) {
                continue;
            }

            var members = type.GetMembers().Where(m =>
            // Not for fields
            m.Kind != SymbolKind.Field
            // Remove implicitly declared
            && !m.IsImplicitlyDeclared
            // Remove Primary constructor
            && !m.DeclaringSyntaxReferences.Any(sr => sr.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax or StructDeclarationSyntax)).ToArray();
            var location = fieldSymbol.Locations[0];

            // Location check
            if (members.Length == 0 || location.SourceSpan.Start <= members.Min(m => m.Locations[0].SourceSpan.Start)) {
                continue;
            }

            // Report diagnostic
            context.ReportDiagnostic(Diagnostic.Create(rule, location, fieldSymbol.Name));
        }
    }
}
