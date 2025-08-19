namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0017PreferTypeNameNewAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0017";
    private const string Category = "Style";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0017AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0017AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0017AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, BaseUrl + DiagnosticId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        => context.RegisterSyntaxNodeAction(VariableDeclarationAction, SyntaxKind.VariableDeclaration);

    private static void VariableDeclarationAction(SyntaxNodeAnalysisContext context) {
        var node = (VariableDeclarationSyntax)context.Node;

        if (!node.Type.IsVar || node.Variables.Count != 1) {
            return;
        }

        var initializer = node.Variables.Single().Initializer;

        if (initializer is null) {
            return;
        }

        var initializerValue = initializer.Value as BaseObjectCreationExpressionSyntax;

        if (initializerValue is ImplicitObjectCreationExpressionSyntax || initializerValue is not ObjectCreationExpressionSyntax oces) {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, node.Type.GetLocation(), oces.Type.ToString()));
    }
}
