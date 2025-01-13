using Microsoft.CodeAnalysis.Text;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0008DontRepeatNegatedPatternAnalyzer : DiagnosticAnalyzer {
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
        // Configure generated code analysis
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution
        context.EnableConcurrentExecution();

        // Register syntax node action
        context.RegisterSyntaxNodeAction(analyzeNotPattern, SyntaxKind.NotPattern);
    }

    private static void analyzeNotPattern(SyntaxNodeAnalysisContext context) {
        var syntax = (UnaryPatternSyntax)context.Node;

        if (syntax.Pattern is not UnaryPatternSyntax) {
            return;
        }

        if (syntax.Parent is UnaryPatternSyntax) {
            return;
        }

        var firstLocation = syntax.GetLocation().SourceSpan.Start;
        var nonFirstLocation = syntax.DescendantNodes().Where(n => n is not UnaryPatternSyntax).Min(n => n.GetLocation().SourceSpan.Start);

        context.ReportDiagnostic(Diagnostic.Create(rule, Location.Create(syntax.SyntaxTree, TextSpan.FromBounds(firstLocation, nonFirstLocation - 1))));
    }
}
