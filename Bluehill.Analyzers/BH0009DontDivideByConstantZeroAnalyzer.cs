namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0009DontDivideByConstantZeroAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0009";
    private const string Category = "Reliability";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0009AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0009AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0009AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description,
        $"{BaseUrl}BH0009");

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

        var leftType = operation.LeftOperand.Type!;

        // The left value is not integer or decimal type
        if (!IsIntegerOrDecimalType(leftType)) {
            return;
        }

        var rightConstant = operation.RightOperand.ConstantValue;

        // The right value is not a constant
        if (!rightConstant.HasValue) {
            return;
        }

        // The right value is not zero
        if (!IsZero(rightConstant.Value)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation()));
    }

    private static bool IsIntegerOrDecimalType(ITypeSymbol typeSymbol)
        => typeSymbol.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
            or SpecialType.System_Int16 or SpecialType.System_UInt16
            or SpecialType.System_Int32 or SpecialType.System_UInt32
            or SpecialType.System_Int64 or SpecialType.System_UInt64
            or SpecialType.System_Decimal;

    private static bool IsZero(object? value)
        => value is byte and 0 or sbyte and 0
            or short and 0 or ushort and 0
            or int and 0 or uint and 0
            or long and 0 or ulong and 0
            or decimal and 0.0m;
}
