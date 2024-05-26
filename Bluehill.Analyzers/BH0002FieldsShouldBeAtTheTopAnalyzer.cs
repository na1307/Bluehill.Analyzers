using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BH0002FieldsShouldBeAtTheTopAnalyzer : DiagnosticAnalyzer {
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
                var location = fieldSymbol.Locations[0];
                var members = fieldSymbol.ContainingType.GetMembers().Where(m => !m.IsImplicitlyDeclared && m.Kind != SymbolKind.Field).ToArray();

                if (members.Length != 0 && location.SourceSpan.Start > members.Min(m => m.Locations[0].SourceSpan.Start)) {
                    context.ReportDiagnostic(Diagnostic.Create(rule, location, fieldSymbol.Name));
                }
            }
        }
    }
}
