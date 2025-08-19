namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0018PreferPatternMatchingAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0018";
    private const string Category = "Style";

    private static readonly LocalizableResourceString
        Title = new(nameof(Resources.BH0018AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0018AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0018AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, BaseUrl + DiagnosticId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        => context.RegisterSyntaxNodeAction(SyntaxNodeAction, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context) {
        var node = (BinaryExpressionSyntax)context.Node;
        var leftIsNullLiteral = node.Left is LiteralExpressionSyntax leftLes && leftLes.IsKind(SyntaxKind.NullLiteralExpression);
        var rightIsNullLiteral = node.Right is LiteralExpressionSyntax rightLes && rightLes.IsKind(SyntaxKind.NullLiteralExpression);

        if (!leftIsNullLiteral && !rightIsNullLiteral) {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
    }
}
