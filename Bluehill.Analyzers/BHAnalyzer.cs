namespace Bluehill.Analyzers;

public abstract class BHAnalyzer : DiagnosticAnalyzer {
    protected const string BaseUrl = "https://na1307.github.io/Bluehill.Analyzers/";

    public override void Initialize(AnalysisContext context) {
        // Configure generated code analysis
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution
        context.EnableConcurrentExecution();
    }
}
