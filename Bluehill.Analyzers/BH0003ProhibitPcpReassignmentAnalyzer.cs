namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0003ProhibitPcpReassignmentAnalyzer : BHAnalyzer {
    public const string DiagnosticId = "BH0003";
    private const string Category = "Design";

    private static readonly LocalizableResourceString Title = new(nameof(Resources.BH0003AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString MessageFormat = new(nameof(Resources.BH0003AnalyzerMessageFormat), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableResourceString Description = new(nameof(Resources.BH0003AnalyzerDescription), Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description,
        $"{BaseUrl}BH0003");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context) {
        base.Initialize(context);

        // Register for different kinds of assignment operations
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.CompoundAssignment);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.CoalesceAssignment);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.DeconstructionAssignment);

        // Also handle increment and decrement operations (x++ / x--)
        context.RegisterOperationAction(AnalyzeIncrementOrDecrement, OperationKind.Increment);
        context.RegisterOperationAction(AnalyzeIncrementOrDecrement, OperationKind.Decrement);
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context) {
        // All of the following are IAssignmentOperation: =, +=, ??=, etc.
        var operation = (IAssignmentOperation)context.Operation;
        var target = operation.Target;

        if (target is not ITupleOperation) {
            CheckTargetAndReport(target, context);
        } else {
            CheckTuple(target, context);
        }

        static void CheckTuple(IOperation target, OperationAnalysisContext context) {
            if (target is ITupleOperation tuple) {
                foreach (var element in tuple.Elements) {
                    CheckTuple(element, context);
                }
            } else {
                CheckTargetAndReport(target, context);
            }
        }
    }

    private static void AnalyzeIncrementOrDecrement(OperationAnalysisContext context) {
        // Increment/decrement are IIncrementOrDecrementOperation
        var operation = (IIncrementOrDecrementOperation)context.Operation;

        CheckTargetAndReport(operation.Target, context);
    }

    private static void CheckTargetAndReport(IOperation target, OperationAnalysisContext context) {
        // Check if the target is a parameter reference
        if (target is not IParameterReferenceOperation parameterRef) {
            return;
        }

        // Check if the parameter is a constructor parameter
        if (parameterRef.Parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } ctor) {
            return;
        }

        // Check if the constructor is a primary constructor
        if (!ctor.DeclaringSyntaxReferences.Any(sr =>
                sr.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax or StructDeclarationSyntax)) {
            return;
        }

        // Report the diagnostic: reassigning primary constructor parameter is not allowed.
        context.ReportDiagnostic(Diagnostic.Create(Rule, target.Syntax.GetLocation(), parameterRef.Parameter.Name));
    }
}
