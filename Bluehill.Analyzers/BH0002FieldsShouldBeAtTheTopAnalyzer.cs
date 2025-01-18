namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0002FieldsShouldBeAtTheTopAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0002";
    private const string Category = "Style";

    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerMessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.BH0002AnalyzerDescription), Resources.ResourceManager,
            typeof(Resources));

    private static readonly DiagnosticDescriptor Rule =
        new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description,
            "https://na1307.github.io/Bluehill.Analyzers/BH0002");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register semantic model action
        context.RegisterSemanticModelAction(SemanticModelAction);
    }

    private static void SemanticModelAction(SemanticModelAnalysisContext context) {
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
#pragma warning disable SA1515
                // Not for fields
                m.Kind != SymbolKind.Field
                // Remove implicitly declared
                && !m.IsImplicitlyDeclared
                // Remove Primary constructor
                && !m.DeclaringSyntaxReferences.Any(sr =>
                    sr.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax or StructDeclarationSyntax)).ToArray();
#pragma warning restore SA1515
            var location = fieldSymbol.Locations[0];

            // Location check
            if (members.Length == 0 || location.SourceSpan.Start <= members.Min(m => m.Locations[0].SourceSpan.Start)) {
                continue;
            }

            // Report diagnostic
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, fieldSymbol.Name));
        }
    }
}
