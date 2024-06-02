using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Bluehill.Analyzers;

//[DiagnosticAnalyzer(LanguageNames.CSharp)]
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
            if (model.GetDeclaredSymbol(variable, token) is IFieldSymbol fieldSymbol) {
                var type = fieldSymbol.ContainingType;

                // Suppress on record types (I don't know how to handle the record's primary constructor.)
                if (!type.GetMembers().Any(m => m.Name == "<Clone>$")) {
                    var members = type.GetMembers().Where(m =>
                    // Remove implicitly declared
                    !m.IsImplicitlyDeclared
                    // Only fields
                    && m.Kind != SymbolKind.Field
                    // Remove Primary constructor (Its location is the same as that of the type definition.)
                    && m.Locations[0].SourceSpan.Start != type.Locations[0].SourceSpan.Start).ToArray();
                    var location = fieldSymbol.Locations[0];

                    if (members.Length != 0 && location.SourceSpan.Start > members.Min(m => m.Locations[0].SourceSpan.Start)) {
                        context.ReportDiagnostic(Diagnostic.Create(rule, location, fieldSymbol.Name));
                    }
                }
            }
        }
    }
}
