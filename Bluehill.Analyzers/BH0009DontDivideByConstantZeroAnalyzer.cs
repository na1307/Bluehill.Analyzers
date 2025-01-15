namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0009DontDivideByConstantZeroAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0009";
    private const string category = "Reliability";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0009AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0009AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0009AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Error, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0009");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register operation action
        context.RegisterOperationAction(binaryOperationAction, OperationKind.Binary);
    }

    private static void binaryOperationAction(OperationAnalysisContext context) {
        var operation = (IBinaryOperation)context.Operation;

        // This is not a division operation
        if (operation.OperatorKind is not BinaryOperatorKind.Divide and not BinaryOperatorKind.Remainder) {
            return;
        }

        var rightConstant = operation.RightOperand.ConstantValue;

        // The value is not a constant
        if (!rightConstant.HasValue) {
            return;
        }

        // The value is not zero
        if (!isZero(rightConstant.Value)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(rule, operation.Syntax.GetLocation()));
    }

    private static bool isZero(object? value) =>
        (value is byte b && b == 0) || (value is sbyte sb && sb == 0)
        || (value is short s && s == 0) || (value is ushort us && us == 0)
        || (value is int i && i == 0) || (value is uint ui && ui == 0)
        || (value is long l && l == 0) || (value is ulong ul && ul == 0);
}
