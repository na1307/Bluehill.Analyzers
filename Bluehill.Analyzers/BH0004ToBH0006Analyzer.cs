﻿namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0004ToBH0006Analyzer : BHAnalyzer {
    // BH0004
    public const string DiagnosticIdBH0004 = "BH0004";
    private const string categoryBH0004 = "Usage";
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
    private const string categoryBH0005 = "Usage";
    private static readonly LocalizableString titleBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormatBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString descriptionBH0005 =
        new LocalizableResourceString(nameof(Resources.BH0005AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor ruleBH0005 =
        new(DiagnosticIdBH0005, titleBH0005, messageFormatBH0005, categoryBH0005, DiagnosticSeverity.Error, true, descriptionBH0005, "https://na1307.github.io/Bluehill.Analyzers/BH0005");

    // BH0006
    public const string DiagnosticIdBH0006 = "BH0006";
    private const string categoryBH0006 = "Usage";
    private static readonly LocalizableString titleBH0006 =
        new LocalizableResourceString(nameof(Resources.BH0006AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormatBH0006 =
        new LocalizableResourceString(nameof(Resources.BH0006AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString descriptionBH0006 =
        new LocalizableResourceString(nameof(Resources.BH0006AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor ruleBH0006 =
        new(DiagnosticIdBH0006, titleBH0006, messageFormatBH0006, categoryBH0006, DiagnosticSeverity.Error, true, descriptionBH0006, "https://na1307.github.io/Bluehill.Analyzers/BH0006");

    // Supported diagnostics
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ruleBH0004, ruleBH0005, ruleBH0006];

    // Initialize the analyzer
    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register compilation start action
        context.RegisterCompilationStartAction(register);
    }

    // Register actions to be performed at the start of compilation
    private static void register(CompilationStartAnalysisContext context) {
        var ixs = context.Compilation.GetTypeByMetadataName("System.Xml.Serialization.IXmlSerializable");

        // IXmlSerializable not found (might be .NET Standard 1.0)
        if (ixs is null) {
            return;
        }

        var ixsgs = (IMethodSymbol)ixs.GetMembers("GetSchema").Single();

        context.RegisterSyntaxNodeAction(context => analyzeMethod(context, ixs, ixsgs), SyntaxKind.MethodDeclaration);
        context.RegisterOperationAction(context => analyzeInvocation(context, ixs, ixsgs), OperationKind.Invocation);
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
                context.ReportDiagnostic(ruleBH0004, methodSymbol.Locations[0]);
            }

            // Report BH0005 if the method is abstract or its return value is not null
            if (methodSymbol.IsAbstract || isReturnValueNotNull(methodDeclaration, model)) {
                var location = methodDeclaration.DescendantNodes().FirstOrDefault(n => n is BlockSyntax or ArrowExpressionClauseSyntax)?.GetLocation() ?? methodDeclaration.GetLocation();

                context.ReportDiagnostic(ruleBH0005, location);
            }
        }
    }

    private static void analyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol ixs, IMethodSymbol ixsgs) {
        var operation = (IInvocationOperation)context.Operation;
        var targetMethod = operation.TargetMethod;

        if (SymbolEqualityComparer.Default.Equals(targetMethod, ixsgs) || isGetSchema(targetMethod, ixs)) {
            context.ReportDiagnostic(Diagnostic.Create(ruleBH0006, operation.Syntax.GetLocation()));
        }
    }

    // Check if the method is GetSchema from IXmlSerializable
    private static bool isGetSchema(IMethodSymbol method, INamedTypeSymbol ixs) {
        var hasIxs = method.ImplementsInterfaceMember(ixs, true);

        // We have already verified that this method is IXmlSerializable.
        // We use Contains rather than Equals to detect explicit interface implementations.
        var isGs = method.Name.Contains("GetSchema");

        return hasIxs && isGs;
    }

    // Check if the return value of the method is not null
    private static bool isReturnValueNotNull(MethodDeclarationSyntax methodDeclaration, SemanticModel model) => methodDeclaration.DescendantNodes()
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
