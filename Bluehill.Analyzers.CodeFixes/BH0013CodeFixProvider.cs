namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0013CodeFixProvider))]
[Shared]
public sealed class BH0013CodeFixProvider : BHCodeFixProvider<LambdaExpressionSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0013LambdaCanBeMadeStaticAnalyzer.DiagnosticId];

    protected override CodeAction CreateCodeAction(Document document, LambdaExpressionSyntax declaration, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0013CodeFixTitle, _ => MakeLambdaStatic(document, declaration, root),
            nameof(CodeFixResources.BH0013CodeFixTitle));

    private static Task<Document> MakeLambdaStatic(Document document, LambdaExpressionSyntax lambda, SyntaxNode root) {
        var newModifier = lambda.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(lambda, lambda.WithModifiers(newModifier))));
    }
}
