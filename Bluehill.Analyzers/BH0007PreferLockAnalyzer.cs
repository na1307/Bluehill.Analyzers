namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0007PreferLockAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0007";
    private const string category = "Performance";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0007AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0007AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0007AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Warning, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0007");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register compilation start action
        context.RegisterCompilationStartAction(compilationStartAction);
    }

    private static void compilationStartAction(CompilationStartAnalysisContext context) {
        var lockType = context.Compilation.GetTypeByMetadataName("System.Threading.Lock");

        // Lock is not available because it is .NET 8 or lower.
        if (lockType is null) {
            return;
        }

        var objectType = context.Compilation.GetTypeByMetadataName("System.Object")!;

        context.RegisterOperationAction(context => lockOperationAction(context, objectType), OperationKind.Lock);
    }

    private static void lockOperationAction(OperationAnalysisContext context, INamedTypeSymbol objectType) {
        var lockOperation = (ILockOperation)context.Operation;
        var lockedType = lockOperation.LockedValue.Type!;

        // Whether the locked type is System.Object
        if (!SymbolEqualityComparer.Default.Equals(lockedType, objectType)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(rule, lockOperation.LockedValue.Syntax.GetLocation()));
    }
}
