namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0007PreferLockAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0007";
    private const string Category = "Performance";

    private static readonly LocalizableResourceString Title =
        new(nameof(Resources.BH0007AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat =
        new(nameof(Resources.BH0007AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString Description =
        new(nameof(Resources.BH0007AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule =
        new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, $"{BaseUrl}BH0007");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register compilation start action
        context.RegisterCompilationStartAction(CompilationStartAction);
    }

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        var lockType = context.Compilation.GetTypeByMetadataName("System.Threading.Lock");

        // Lock is not available because it is .NET 8 or lower.
        if (lockType is null) {
            return;
        }

        var objectType = context.Compilation.GetTypeByMetadataName("System.Object")!;

        context.RegisterOperationAction(ac => LockOperationAction(ac, objectType), OperationKind.Lock);
    }

    private static void LockOperationAction(OperationAnalysisContext context, INamedTypeSymbol objectType) {
        var lockOperation = (ILockOperation)context.Operation;
        var lockedType = lockOperation.LockedValue.Type!;

        // Whether the locked type is System.Object
        if (!SymbolEqualityComparer.Default.Equals(lockedType, objectType)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(Rule, lockOperation.LockedValue.Syntax.GetLocation()));
    }
}
