namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0016EegLangVersionAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0016";
    private const string Category = "Usage";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0016AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0016AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0016AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true,
        Description, BaseUrl + DiagnosticId);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context) => context.RegisterCompilationStartAction(CompilationStartAction);

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        var compilation = (CSharpCompilation)context.Compilation;
        var langVersion = compilation.LanguageVersion;

        if (langVersion >= LanguageVersion.CSharp10) {
            return;
        }

        var eea = compilation.GetTypeByMetadataName("Bluehill.EnumExtensionsAttribute");

        if (eea is null) {
            return;
        }

        context.RegisterSyntaxNodeAction(c => SyntaxNodeAction(c, eea, langVersion), SyntaxKind.EnumDeclaration);
    }

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context, INamedTypeSymbol eea, LanguageVersion langVersion) {
        var syntax = (EnumDeclarationSyntax)context.Node;

        var attribute = syntax.AttributeLists.SelectMany(al => al.Attributes.Where(a =>
                SEC.Default.Equals(context.SemanticModel.GetSymbol(a, context.CancellationToken)!.ContainingType, eea)))
            .SingleOrDefault();

        if (attribute is null) {
            return;
        }

        context.ReportDiagnostic(Rule, attribute, langVersion.ToDisplayString());
    }
}
