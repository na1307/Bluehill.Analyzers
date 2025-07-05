namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BHDECORATORAnalyzer : BHAnalyzer {
    public const string DiagnosticIdBHDECORATOR0002 = "BHDECORATOR0002";
    private const string CategoryBHDECORATOR0002 = "Decorator";

    private static readonly DiagnosticDescriptor RuleBHDECORATOR0002 = new(DiagnosticIdBHDECORATOR0002,
        "Method not supported",
        "Method not supported",
        CategoryBHDECORATOR0002,
        DiagnosticSeverity.Warning,
        true,
        "Only public static methods with no parameters or type parameters are supported. Decorator was not generated.",
        BaseUrl + DiagnosticIdBHDECORATOR0002);

    public const string DiagnosticIdBHDECORATOR0003 = "BHDECORATOR0003";
    private const string CategoryBHDECORATOR0003 = "Decorator";

    private static readonly DiagnosticDescriptor RuleBHDECORATOR0003 = new(DiagnosticIdBHDECORATOR0003,
        "Decorator method FQN is invalid",
        "Decorator method FQN is invalid",
        CategoryBHDECORATOR0003,
        DiagnosticSeverity.Warning,
        true,
        "The decorator FQN is invalid or the specified method does not exist. Decorator was not generated.",
        BaseUrl + DiagnosticIdBHDECORATOR0003);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleBHDECORATOR0002, RuleBHDECORATOR0003];

    protected override void RegisterActions(AnalysisContext context) => context.RegisterCompilationStartAction(CompilationStartAction);

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        var da = context.Compilation.GetTypeByMetadataName("Bluehill.DecoratorAttribute");

        if (da is null) {
            return;
        }

        context.RegisterSyntaxNodeAction(c => SyntaxNodeAction(c, da), SyntaxKind.MethodDeclaration);
    }

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context, INamedTypeSymbol da) {
        var syntax = (MethodDeclarationSyntax)context.Node;

        var decorator = syntax.AttributeLists.SelectMany(al => al.Attributes.Where(a =>
                SEC.Default.Equals(context.SemanticModel.GetSymbol(a, context.CancellationToken)!.ContainingType, da)))
            .SingleOrDefault();

        if (decorator is null) {
            return;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(syntax, context.CancellationToken)!;

        if (symbol.Parameters.Any() || symbol.TypeParameters.Any() || !symbol.IsStatic || symbol.DeclaredAccessibility != Accessibility.Public) {
            context.ReportDiagnostic(RuleBHDECORATOR0002, decorator);
        }

        var decoratorName = decorator.ArgumentList?.Arguments.Select(a => a.ToString().Trim('"')).FirstOrDefault();

        if (decoratorName is null) {
            context.ReportDiagnostic(RuleBHDECORATOR0003, decorator);

            return;
        }

        var splitDecoratorName = decoratorName.Split('.');
        var decoratorMethod = context.Compilation.GetTypeByMetadataName(string.Join(".", splitDecoratorName.Take(splitDecoratorName.Length - 1)))?.GetMembers()
            .OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == splitDecoratorName[splitDecoratorName.Length - 1]);

        if (decoratorMethod is null) {
            context.ReportDiagnostic(RuleBHDECORATOR0003, decorator);
        }
    }
}
