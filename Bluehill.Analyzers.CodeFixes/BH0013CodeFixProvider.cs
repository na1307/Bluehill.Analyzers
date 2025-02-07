namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0013CodeFixProvider))]
[Shared]
public sealed class BH0013CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0013LambdaCanBeMadeStaticAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == BH0013LambdaCanBeMadeStaticAnalyzer.DiagnosticId);
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = (LambdaExpressionSyntax)root!.FindToken(diagnosticSpan.Start).Parent!.Parent!;

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(CodeAction.Create(CodeFixResources.BH0013CodeFixTitle,
            _ => MakeLambdaStatic(context.Document, node, root),
            nameof(CodeFixResources.BH0013CodeFixTitle)), diagnostic);
    }

    private static Task<Document> MakeLambdaStatic(Document document, LambdaExpressionSyntax lambda, SyntaxNode root) {
        var newModifier = lambda.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(lambda, lambda.WithModifiers(newModifier))));
    }
}
