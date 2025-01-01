using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Bluehill.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BH0003ProhibitPCPReassignmentAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = "BH0003";
    private const string category = "Design";
    private static readonly LocalizableString title =
        new LocalizableResourceString(nameof(Resources.BH0003AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString messageFormat =
        new LocalizableResourceString(nameof(Resources.BH0003AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString description =
        new LocalizableResourceString(nameof(Resources.BH0003AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private static readonly DiagnosticDescriptor rule =
        new(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Error, true, description, "https://na1307.github.io/Bluehill.Analyzers/BH0003");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rule];

    public override void Initialize(AnalysisContext context) {
        // Ignore generated code and allow concurrent analysis
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Register for different kinds of assignment operations
        context.RegisterOperationAction(analyzeAssignment, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(analyzeAssignment, OperationKind.CompoundAssignment);
        context.RegisterOperationAction(analyzeAssignment, OperationKind.CoalesceAssignment);
        context.RegisterOperationAction(analyzeAssignment, OperationKind.DeconstructionAssignment);

        // Also handle increment and decrement operations (x++ / x--)
        context.RegisterOperationAction(analyzeIncrementOrDecrement, OperationKind.Increment);
        context.RegisterOperationAction(analyzeIncrementOrDecrement, OperationKind.Decrement);
    }

    private static void analyzeAssignment(OperationAnalysisContext context) {
        // All of the following are IAssignmentOperation: =, +=, ??=, etc.
        var operation = (IAssignmentOperation)context.Operation;
        var target = operation.Target;

        if (target is not ITupleOperation) {
            checkTargetAndReport(target, context);
        } else {
            checkTuple(target, context);
        }

        static void checkTuple(IOperation target, OperationAnalysisContext context) {
            if (target is ITupleOperation tuple) {
                foreach (var element in tuple.Elements) {
                    checkTuple(element, context);
                }
            } else {
                checkTargetAndReport(target, context);
            }
        }
    }

    private static void analyzeIncrementOrDecrement(OperationAnalysisContext context) {
        // Increment/decrement are IIncrementOrDecrementOperation
        var operation = (IIncrementOrDecrementOperation)context.Operation;

        checkTargetAndReport(operation.Target, context);
    }

    private static void checkTargetAndReport(IOperation target, OperationAnalysisContext context) {
        // Check if the target is a parameter reference
        if (target is not IParameterReferenceOperation parameterRef) {
            return;
        }

        // Check if the parameter is a constructor parameter
        if (parameterRef.Parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } ctor) {
            return;
        }

        // Check if the constructor is a primary constructor
        if (!ctor.DeclaringSyntaxReferences.Any(sr => sr.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax or StructDeclarationSyntax)) {
            return;
        }

        // Report the diagnostic: reassigning primary constructor parameter is not allowed.
        context.ReportDiagnostic(Diagnostic.Create(rule, target.Syntax.GetLocation(), ((IParameterReferenceOperation)target).Parameter.Name));
    }
}
