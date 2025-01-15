namespace Bluehill.Analyzers;

public abstract class BHAnalyzer : DiagnosticAnalyzer {
    public override void Initialize(AnalysisContext context) {
        // Configure generated code analysis
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution
        context.EnableConcurrentExecution();
    }
}
