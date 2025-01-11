﻿namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0004AndBH0005Analyzer : DiagnosticAnalyzer {
    // BH0004
    public const string DiagnosticIdBH0004 = "BH0004";
    private const string categoryBH0004 = "Design";
    private static readonly LocalizableString titleBH0004 =
        new LocalizableResourceString(nameof(Resources.BH0004AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormatBH0004 =
        new LocalizableResourceString(nameof(Resources.BH0004AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString descriptionBH0004 =
        new LocalizableResourceString(nameof(Resources.BH0004AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor ruleBH0004 =
        new(DiagnosticIdBH0004, titleBH0004, messageFormatBH0004, categoryBH0004, DiagnosticSeverity.Error, true, descriptionBH0004, "https://na1307.github.io/Bluehill.Analyzers/BH0004");

    // BH0005
    public const string DiagnosticIdBH0005 = "BH0005";
    private const string categoryBH0005 = "Design";
    private static readonly LocalizableString titleBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormatBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString descriptionBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor ruleBH0005 =
        new(DiagnosticIdBH0005, titleBH0005, messageFormatBH0005, categoryBH0005, DiagnosticSeverity.Error, true, descriptionBH0005, "https://na1307.github.io/Bluehill.Analyzers/BH0005");

    // Supported diagnostics
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ruleBH0004, ruleBH0005];

    // Initialize the analyzer
    public override void Initialize(AnalysisContext context) {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(register);
    }

    // Register actions to be performed at the start of compilation
    private static void register(CompilationStartAnalysisContext context) {
        var compilation = context.Compilation;
        var ixs = compilation.GetTypeByMetadataName("System.Xml.Serialization.IXmlSerializable") ?? throw new InvalidOperationException("Something went wrong");
        var ixsgs = (IMethodSymbol)ixs.GetMembers("GetSchema").Single();

        context.RegisterSyntaxNodeAction(ac => analyzeMethod(ac, ixs, ixsgs), SyntaxKind.MethodDeclaration);
    }

    // Analyze method declarations
    private static void analyzeMethod(SyntaxNodeAnalysisContext context, INamedTypeSymbol ixs, IMethodSymbol ixsgs) {
        var model = context.SemanticModel;
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var methodSymbol = model.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) ?? throw new InvalidOperationException("Something went wrong");

        // Check if the method is GetSchema from IXmlSerializable
        if (isGetSchema(methodSymbol, ixs)) {
            // Report BH0004 if the method does not explicitly implement IXmlSerializable.GetSchema
            if (!methodSymbol.ExplicitInterfaceImplementations.Any(i => SymbolEqualityComparer.Default.Equals(i, ixsgs))) {
                context.ReportDiagnostic(Diagnostic.Create(ruleBH0004, methodSymbol.Locations[0]));
            }

            // Report BH0005 if the method is abstract or its return value is not null
            if (methodSymbol.IsAbstract || isReturnValueNotNull(methodDeclaration, model)) {
                var location = methodDeclaration.DescendantNodes().FirstOrDefault(n => n is BlockSyntax or ArrowExpressionClauseSyntax)?.GetLocation() ?? methodSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(ruleBH0005, location));
            }
        }
    }

    // Check if the method is GetSchema from IXmlSerializable
    private static bool isGetSchema(IMethodSymbol method, INamedTypeSymbol ixs) {
        var hasIxs = method.ContainingType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, ixs));
        var isInIxs = ixs.GetMembers().Select(method.ContainingType.FindImplementationForInterfaceMember).Any(im => SymbolEqualityComparer.Default.Equals(method, im));

        // We have already verified that this method is IXmlSerializable.
        // We use Contains rather than Equals to detect explicit interface implementations.
        var isGs = method.Name.Contains("GetSchema");

        return hasIxs && isInIxs && isGs;
    }

    // Check if the return value of the method is not null
    private static bool isReturnValueNotNull(MethodDeclarationSyntax methodDeclaration, SemanticModel model) => methodDeclaration.DescendantNodes()
        .Where(n => n is ReturnStatementSyntax or ArrowExpressionClauseSyntax or ThrowStatementSyntax or ThrowExpressionSyntax)
        .Select(s => s switch {
            ReturnStatementSyntax statement => statement.Expression,
            ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause.Expression,
            ThrowStatementSyntax throwStatement => throwStatement.Expression,
            ThrowExpressionSyntax throwExpression => throwExpression.Expression,
            _ => throw new InvalidOperationException("Something went wrong")
        }).Where(e => e is not null).Select(e => e!).Any(e => {
            var constantValue = model.GetConstantValue(e);

            return !constantValue.HasValue || constantValue.Value is not null;
        });
}
