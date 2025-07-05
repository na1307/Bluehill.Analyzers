using Microsoft.CodeAnalysis.Text;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0008DontRepeatNegatedPatternAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0008";
    private const string Category = "Design";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0008AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0008AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0008AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, BaseUrl + DiagnosticId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        // Register syntax node action
        => context.RegisterSyntaxNodeAction(AnalyzeNotPattern, SyntaxKind.NotPattern);

    private static void AnalyzeNotPattern(SyntaxNodeAnalysisContext context) {
        var syntax = (UnaryPatternSyntax)context.Node;

        // Sub pattern is actual pattern
        if (syntax.Pattern is not UnaryPatternSyntax) {
            return;
        }

        // Parent is negated pattern. It is already handled.
        if (syntax.Parent is UnaryPatternSyntax) {
            return;
        }

        var firstLocation = syntax.SpanStart;
        var nonFirstLocation = syntax.DescendantNodes().Where(n => n is not UnaryPatternSyntax).Min(n => n.SpanStart);

        // Report diagnostic
        context.ReportDiagnostic(Rule,
            Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(firstLocation, nonFirstLocation - 1)));
    }
}
