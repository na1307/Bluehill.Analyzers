namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0014ToBH0015Analyzer : BHAnalyzer {
    // BH0014
    public const string DiagnosticIdBH0014 = "BH0014";
    private const string CategoryBH0014 = "Performance";

    private static readonly LocalizableResourceString TitleBH0014 = new(nameof(Resources.BH0014AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0014 = new(nameof(Resources.BH0014AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0014 = new(nameof(Resources.BH0014AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0014 = new(DiagnosticIdBH0014, TitleBH0014, MessageFormatBH0014, CategoryBH0014,
        DiagnosticSeverity.Warning, true, DescriptionBH0014, $"{BaseUrl}BH0014");

    // BH0015
    public const string DiagnosticIdBH0015 = "BH0015";
    private const string CategoryBH0015 = "Performance";

    private static readonly LocalizableResourceString TitleBH0015 = new(nameof(Resources.BH0015AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormatBH0015 = new(nameof(Resources.BH0015AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableResourceString DescriptionBH0015 = new(nameof(Resources.BH0015AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor RuleBH0015 = new(DiagnosticIdBH0015, TitleBH0015, MessageFormatBH0015, CategoryBH0015,
        DiagnosticSeverity.Warning, true, DescriptionBH0015, $"{BaseUrl}BH0015");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleBH0014, RuleBH0015];

    protected override void RegisterActions(AnalysisContext context)
        // Register compilation start action
        => context.RegisterCompilationStartAction(CompilationStartAction);

    private static void CompilationStartAction(CompilationStartAnalysisContext context) {
        var compilation = context.Compilation;
        var systemEnum = compilation.GetTypeByMetadataName("System.Enum")!;
        var enumToString = (IMethodSymbol)systemEnum.GetMembers("ToString").Single(m => m is IMethodSymbol { Parameters.Length: 0 });
        var enumHasFlag = (IMethodSymbol?)systemEnum.GetMembers("HasFlag").SingleOrDefault();

        context.RegisterOperationAction(oac => OperationAction(oac, compilation, enumToString, enumHasFlag), OperationKind.Invocation);
    }

    private static void OperationAction(
        OperationAnalysisContext context,
        Compilation compilation,
        IMethodSymbol enumToString,
        IMethodSymbol? enumHasFlag) {
        var operation = (IInvocationOperation)context.Operation;
        var targetMethod = operation.TargetMethod;

        if (!SEC.Default.Equals(targetMethod, enumToString) && !SEC.Default.Equals(targetMethod, enumHasFlag)) {
            return;
        }

        var type = operation.ChildOperations.First().Type!;
        var fqnName = type.ToDisplayString(FqnFormat);
        var namespaceName = type.ContainingNamespace?.MetadataName;
        var typeName = GetTypeName(fqnName, namespaceName);
        var escapedTypeName = GetEscapedTypeName(typeName);
        var extensionsType = compilation.GetTypeByMetadataName($"{namespaceName}.{escapedTypeName}Extensions");
        var fastMethodName = targetMethod.Name + "Fast";

        if (extensionsType is null) {
            if (!SEC.Default.Equals(type.ContainingAssembly, compilation.Assembly)) {
                return;
            }

            if (GetAccessibility(type) is not (Accessibility.Public or Accessibility.Internal)) {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(RuleBH0015, operation.Syntax.GetLocation(), fastMethodName));

            return;
        }

        if (extensionsType.GetMembers(fastMethodName).SingleOrDefault() is not null) {
            context.ReportDiagnostic(Diagnostic.Create(RuleBH0014, operation.Syntax.GetLocation(), fastMethodName));
        }
    }
}
