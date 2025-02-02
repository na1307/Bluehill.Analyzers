namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0012FieldNameShouldNotConflictAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0012";
    private const string Category = "Design";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0012AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0012AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0012AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, $"{BaseUrl}BH0012");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        // Register syntax node action
        => context.RegisterSyntaxNodeAction(SyntaxNodeAction, SyntaxKind.FieldDeclaration);

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context) {
        var member = (FieldDeclarationSyntax)context.Node;

        // Parent is not a type
        if (member.Parent is not TypeDeclarationSyntax type) {
            return;
        }

        var parameterList = type.ParameterList;

        // This type does not have primary constructor
        if (parameterList is null) {
            return;
        }

        var identifier = member.Declaration.Variables.Single().Identifier;

        // No name conflicts
        if (!parameterList.Parameters.Any(p => p.Identifier.ValueText == identifier.ValueText)) {
            return;
        }

        // Report diagnostic
        context.ReportDiagnostic(Rule, identifier.GetLocation(), identifier);
    }
}
