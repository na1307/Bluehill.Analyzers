namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0001TypesShouldBeSealedAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0001";
    private const string Category = "Design";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0001AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0001AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0001AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, $"{BaseUrl}BH0001");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        // Register compilation start action
        => context.RegisterCompilationStartAction(CompilationStartAction);

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        // Get all named base types
        var baseTypes = context.Compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type, context.CancellationToken)
            .Cast<INamedTypeSymbol>().Select(symbol => symbol.BaseType).ToImmutableArray();

        // Register symbol action
        context.RegisterSymbolAction(ac => SymbolAction(ac, baseTypes), SymbolKind.NamedType);
    }

    private static void SymbolAction(
        SymbolAnalysisContext context,
        ImmutableArray<INamedTypeSymbol?> baseTypes) {
        // Get named type symbol
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Skip static
        if (namedTypeSymbol.IsStatic) {
            return;
        }

        // Skip sealed
        if (namedTypeSymbol.IsSealed) {
            return;
        }

        // Skip abstract
        if (namedTypeSymbol.IsAbstract) {
            return;
        }

        // Skip implicitly declared
        if (namedTypeSymbol.IsImplicitlyDeclared) {
            return;
        }

        // Skip implicit classes
        if (namedTypeSymbol.IsImplicitClass) {
            return;
        }

        // Skip if it is not a class
        if (namedTypeSymbol.TypeKind != TypeKind.Class) {
            return;
        }

        // Skip Top level statement Program class
        if (namedTypeSymbol.Name == WellKnownMemberNames.TopLevelStatementsEntryPointTypeName) {
            return;
        }

        // Check is there are derived classes
        if (baseTypes.Contains(namedTypeSymbol, SEC.Default)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
    }
}
