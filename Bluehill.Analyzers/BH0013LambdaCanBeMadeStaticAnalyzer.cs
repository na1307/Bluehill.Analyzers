namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0013LambdaCanBeMadeStaticAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0013";
    private const string Category = "Performance";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0013AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0013AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0013AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, true,
        Description, $"{BaseUrl}BH0013");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    protected override void RegisterActions(AnalysisContext context)
        // Register syntax node action
        => context.RegisterSyntaxNodeAction(SyntaxNodeAction, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);

    private static void SyntaxNodeAction(SyntaxNodeAnalysisContext context) {
        var lambda = (LambdaExpressionSyntax)context.Node;
        var model = context.SemanticModel;
        var token = context.CancellationToken;
        var lambdaSymbol = model.GetSymbol(lambda, token);
        var operations = lambda.Body.DescendantNodes().Select(node => model.GetOperation(node, token)).ToArray();

        var hasInstanceReference = operations.OfType<IInstanceReferenceOperation>().Any()
            || operations.OfType<ILocalReferenceOperation>().Any(o => !SEC.Default.Equals(o.Local.ContainingSymbol, lambdaSymbol))
            || operations.OfType<IParameterReferenceOperation>().Any(o => !SEC.Default.Equals(o.Parameter.ContainingSymbol, lambdaSymbol))
            || operations.OfType<IFieldReferenceOperation>().Any(o => !o.Field.IsStatic)
            || operations.OfType<IMethodReferenceOperation>().Any(o => !o.Method.IsStatic)
            || operations.OfType<IPropertyReferenceOperation>().Any(o => !o.Property.IsStatic)
            || operations.OfType<IEventReferenceOperation>().Any(o => !o.Event.IsStatic)
            || operations.OfType<IInvocationOperation>().Any(o => !o.TargetMethod.IsStatic);

        if (hasInstanceReference) {
            return;
        }

        context.ReportDiagnostic(Rule, lambda.GetLocation());
    }
}
