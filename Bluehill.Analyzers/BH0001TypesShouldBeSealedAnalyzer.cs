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

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register compilation start action
        context.RegisterCompilationStartAction(CompilationStartAction);
    }

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        // Get all named types and their base types
        var typeAndBase = context.Compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type, context.CancellationToken)
            .Cast<INamedTypeSymbol>().Select(symbol => new KeyValuePair<INamedTypeSymbol, INamedTypeSymbol?>(symbol, symbol.BaseType))
            .ToImmutableDictionary(SymbolEqualityComparer.Default);

        // Register symbol action
        context.RegisterSymbolAction(ac => SymbolAction(ac, typeAndBase), SymbolKind.NamedType);
    }

    private static void SymbolAction(
        SymbolAnalysisContext context,
        ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol?> typeAndBase) {
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

        // Skip implicitily declared
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
        if (typeAndBase.Any(ts => SymbolEqualityComparer.Default.Equals(namedTypeSymbol, ts.Value))) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
    }
}
