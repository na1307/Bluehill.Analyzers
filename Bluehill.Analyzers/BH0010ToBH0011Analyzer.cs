using Microsoft.CodeAnalysis.Text;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0010ToBH0011Analyzer : BHAnalyzer {
    // BH0010
    public const string DiagnosticIdBH0010 = "BH0010";
    private const string CategoryBH0010 = "Usage";

    private static readonly LocalizableResourceString TitleBH0010 = new(nameof(Resources.BH0010AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0010 = new(nameof(Resources.BH0010AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0010 = new(nameof(Resources.BH0010AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0010 = new(DiagnosticIdBH0010, TitleBH0010, MessageFormatBH0010, CategoryBH0010,
        DiagnosticSeverity.Warning, true, DescriptionBH0010, BaseUrl + DiagnosticIdBH0010);

    // BH0011
    public const string DiagnosticIdBH0011 = "BH0011";
    private const string CategoryBH0011 = "Usage";

    private static readonly LocalizableResourceString TitleBH0011 = new(nameof(Resources.BH0011AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0011 = new(nameof(Resources.BH0011AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0011 = new(nameof(Resources.BH0011AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0011 = new(DiagnosticIdBH0011, TitleBH0011, MessageFormatBH0011, CategoryBH0011,
        DiagnosticSeverity.Warning, true, DescriptionBH0011, BaseUrl + DiagnosticIdBH0011);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleBH0010, RuleBH0011];

    protected override void RegisterActions(AnalysisContext context)
        // Register compilation start action
        => context.RegisterCompilationStartAction(CompilationStartAction);

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        var spanType = context.Compilation.GetTypeByMetadataName("System.Span`1");

        // Check if Span<T> type is available
        if (spanType is null) {
            return;
        }

        var readOnlySpanType = context.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");

        // Check if ReadOnlySpan<T> type is available
        if (readOnlySpanType is null) {
            return;
        }

        // Register syntax node action
        context.RegisterSyntaxNodeAction(snac => SyntaxNodeAction(snac, spanType, readOnlySpanType),
            SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context, INamedTypeSymbol spanType, INamedTypeSymbol readOnlySpanType) {
        var model = context.SemanticModel;
        var token = context.CancellationToken;
        var node = (BinaryExpressionSyntax)context.Node;
        var operation = (IBinaryOperation)model.GetOperation(node, token)!;
        var leftType = (INamedTypeSymbol)operation.LeftOperand.Type!;

        // Check if left operand is Span<T> or ReadOnlySpan<T>
        if (!MetadataNameEqualityComparer<INamedTypeSymbol>.Instance.Equals(leftType, spanType)
            && !MetadataNameEqualityComparer<INamedTypeSymbol>.Instance.Equals(leftType, readOnlySpanType)) {
            return;
        }

        var rightSyntax = operation.RightOperand.Syntax;

        // Check if right operand is constant value and collection has non-constant
        var hasNonConstant = rightSyntax.Kind() is not (SyntaxKind.StringLiteralExpression or SyntaxKind.CollectionExpression
            or SyntaxKind.ArrayCreationExpression or SyntaxKind.ImplicitArrayCreationExpression) || rightSyntax switch {
                CollectionExpressionSyntax ce => ce.Elements.Any(e
                    => !model.GetConstantValue(e.DescendantNodes().Single(), token).HasValue),
                ArrayCreationExpressionSyntax ace => ace.Initializer?.Expressions.Any(e
                    => !model.GetConstantValue(e, token).HasValue) ?? false,
                ImplicitArrayCreationExpressionSyntax iace => iace.Initializer.Expressions.Any(e
                    => !model.GetConstantValue(e, token).HasValue),
                _ => false
            };

        var start = node.OperatorToken.Span.Start;
        var end = node.Right.Span.End;

        context.ReportDiagnostic(!hasNonConstant ? RuleBH0010 : RuleBH0011, Location.Create(node.SyntaxTree, TextSpan.FromBounds(start, end)));
    }
}
