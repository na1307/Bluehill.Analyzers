using Microsoft.CodeAnalysis.Text;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0008DontRepeatNegatedPatternAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0008";
    private const string category = "Design";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0008AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0008AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0008AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Warning, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0008");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register syntax node action
        context.RegisterSyntaxNodeAction(analyzeNotPattern, SyntaxKind.NotPattern);
    }

    private static void analyzeNotPattern(SyntaxNodeAnalysisContext context) {
        var syntax = (UnaryPatternSyntax)context.Node;

        // Subpattern is actual pattern
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
        context.ReportDiagnostic(rule, Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(firstLocation, nonFirstLocation - 1)));
    }
}
