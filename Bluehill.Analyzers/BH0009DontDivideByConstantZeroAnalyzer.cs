namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0009DontDivideByConstantZeroAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0009";
    private const string Category = "Reliability";

    private static readonly LocalizableResourceString Title =
        new(nameof(Resources.BH0009AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat =
        new(nameof(Resources.BH0009AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString Description =
        new(nameof(Resources.BH0009AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule =
        new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, $"{BaseUrl}BH0009");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register operation action
        context.RegisterOperationAction(BinaryOperationAction, OperationKind.Binary);
    }

    private static void BinaryOperationAction(OperationAnalysisContext context) {
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
        if (!IsZero(rightConstant.Value)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation()));
    }

    private static bool IsZero(object? value) =>
        (value is byte b && b == 0) || (value is sbyte sb && sb == 0)
                                    || (value is short s && s == 0) || (value is ushort us && us == 0)
                                    || (value is int i && i == 0) || (value is uint ui && ui == 0)
                                    || (value is long l && l == 0) || (value is ulong ul && ul == 0);
}
