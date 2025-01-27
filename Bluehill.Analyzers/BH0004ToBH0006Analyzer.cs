namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0004ToBH0006Analyzer : BHAnalyzer {
    // BH0004
    public const string DiagnosticIdBH0004 = "BH0004";
    private const string CategoryBH0004 = "Usage";

    private static readonly LocalizableResourceString TitleBH0004 = new(nameof(Resources.BH0004AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0004 = new(nameof(Resources.BH0004AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0004 = new(nameof(Resources.BH0004AnalyzerDescription),
        Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0004 = new(DiagnosticIdBH0004, TitleBH0004, MessageFormatBH0004, CategoryBH0004,
        DiagnosticSeverity.Error, true, DescriptionBH0004, $"{BaseUrl}BH0004");

    // BH0005
    public const string DiagnosticIdBH0005 = "BH0005";
    private const string CategoryBH0005 = "Usage";

    private static readonly LocalizableResourceString TitleBH0005 = new(nameof(Resources.BH0005AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0005 = new(nameof(Resources.BH0005AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0005 = new(nameof(Resources.BH0005AnalyzerDescription),
        Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0005 = new(DiagnosticIdBH0005, TitleBH0005, MessageFormatBH0005, CategoryBH0005,
        DiagnosticSeverity.Error, true, DescriptionBH0005, $"{BaseUrl}BH0005");

    // BH0006
    public const string DiagnosticIdBH0006 = "BH0006";
    private const string CategoryBH0006 = "Usage";

    private static readonly LocalizableResourceString TitleBH0006 = new(nameof(Resources.BH0006AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0006 = new(nameof(Resources.BH0006AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0006 = new(nameof(Resources.BH0006AnalyzerDescription),
        Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0006 = new(DiagnosticIdBH0006, TitleBH0006, MessageFormatBH0006, CategoryBH0006,
        DiagnosticSeverity.Error, true, DescriptionBH0006, $"{BaseUrl}BH0006");

    // Supported diagnostics
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleBH0004, RuleBH0005, RuleBH0006];

    // Initialize the analyzer
    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register compilation start action
        context.RegisterCompilationStartAction(Register);
    }

    // Register actions to be performed at the start of compilation
    private static void Register(CompilationStartAnalysisContext context) {
        var ixs = context.Compilation.GetTypeByMetadataName("System.Xml.Serialization.IXmlSerializable");

        // IXmlSerializable not found (might be .NET Standard 1.0)
        if (ixs is null) {
            return;
        }

        var ixsgs = (IMethodSymbol)ixs.GetMembers("GetSchema").Single();

        context.RegisterSyntaxNodeAction(ac => AnalyzeMethod(ac, ixs, ixsgs), SyntaxKind.MethodDeclaration);
        context.RegisterOperationAction(ac => AnalyzeInvocation(ac, ixs, ixsgs), OperationKind.Invocation);
    }

    // Analyze method declarations
    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, INamedTypeSymbol ixs, IMethodSymbol ixsgs) {
        var model = context.SemanticModel;
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var methodSymbol = model.GetDeclaredSymbol(methodDeclaration, context.CancellationToken)
            ?? throw new InvalidOperationException("Something went wrong");

        // Check if the method is GetSchema from IXmlSerializable
        if (IsGetSchema(methodSymbol, ixs)) {
            // Report BH0004 if the method does not explicitly implement IXmlSerializable.GetSchema
            if (!methodSymbol.ExplicitInterfaceImplementations.Any(i => SymbolEqualityComparer.Default.Equals(i, ixsgs))) {
                context.ReportDiagnostic(RuleBH0004, methodSymbol.Locations[0]);
            }

            // Report BH0005 if the method is abstract or its return value is not null
            if (methodSymbol.IsAbstract || IsReturnValueNotNull(methodDeclaration, model)) {
                var location = methodDeclaration.DescendantNodes()
                    .FirstOrDefault(n => n is BlockSyntax or ArrowExpressionClauseSyntax)?.GetLocation() ?? methodDeclaration.GetLocation();

                context.ReportDiagnostic(RuleBH0005, location);
            }
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol ixs, IMethodSymbol ixsgs) {
        var operation = (IInvocationOperation)context.Operation;
        var targetMethod = operation.TargetMethod;

        if (SymbolEqualityComparer.Default.Equals(targetMethod, ixsgs) || IsGetSchema(targetMethod, ixs)) {
            context.ReportDiagnostic(Diagnostic.Create(RuleBH0006, operation.Syntax.GetLocation()));
        }
    }

    // Check if the method is GetSchema from IXmlSerializable
    private static bool IsGetSchema(IMethodSymbol method, INamedTypeSymbol ixs) {
        var hasIxs = method.ImplementsInterfaceMember(ixs, true);

        // We have already verified that this method is IXmlSerializable.
        // We use Contains rather than Equals to detect explicit interface implementations.
        var isGs = method.Name.Contains("GetSchema");

        return hasIxs && isGs;
    }

    // Check if the return value of the method is not null
    private static bool IsReturnValueNotNull(MethodDeclarationSyntax methodDeclaration, SemanticModel model)
        => methodDeclaration.DescendantNodes()
            .Select(s => s switch {
                ReturnStatementSyntax statement => statement.Expression,
                ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause.Expression,
                ThrowStatementSyntax throwStatement => throwStatement.Expression,
                ThrowExpressionSyntax throwExpression => throwExpression.Expression,
                _ => null
            }).Any(e => {
                if (e is null) {
                    return false;
                }

                var constantValue = model.GetConstantValue(e);

                return !constantValue.HasValue || constantValue.Value is not null;
            });
}
