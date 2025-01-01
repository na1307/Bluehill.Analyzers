using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0001TypesShouldBeSealedAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = "BH0001";
    private const string category = "Design";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0001AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0001AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0001AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Warning, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0001");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        // Configure generated code analysis
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution
        context.EnableConcurrentExecution();

        // Register compilation start action
        context.RegisterCompilationStartAction(compilationStartAction);
    }

    private static void compilationStartAction(CompilationStartAnalysisContext context) {
        var token = context.CancellationToken;

        // Get all named types and their base types
        var typeAndBase = context.Compilation.GetSymbolsWithName(_ => true, SymbolFilter.Type, token).Cast<INamedTypeSymbol>()
            .Select(symbol => new KeyValuePair<INamedTypeSymbol, INamedTypeSymbol?>(symbol, symbol.BaseType))
            .ToImmutableDictionary(SymbolEqualityComparer.Default);

        // Register symbol action
        context.RegisterSymbolAction(context => symbolAction(context, typeAndBase), SymbolKind.NamedType);
    }

    private static void symbolAction(SymbolAnalysisContext context, ImmutableDictionary<INamedTypeSymbol, INamedTypeSymbol?> typeAndBase) {
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
        if (!typeAndBase.FirstOrDefault(ts => SymbolEqualityComparer.Default.Equals(namedTypeSymbol, ts.Value))
            .Equals(default(KeyValuePair<INamedTypeSymbol, INamedTypeSymbol?>))) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name));
    }
}
