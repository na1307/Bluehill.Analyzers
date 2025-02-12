namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0014CodeFixProvider))]
[Shared]
public sealed class BH0014CodeFixProvider : BHCodeFixProvider<InvocationExpressionSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0014ToBH0015Analyzer.DiagnosticIdBH0014];

    protected override CodeAction CreateCodeAction(Document document, InvocationExpressionSyntax declaration, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0014CodeFixTitle, _ => SuffixFast(document, declaration, root),
        nameof(CodeFixResources.BH0014CodeFixTitle));

    private static Task<Document> SuffixFast(Document document, InvocationExpressionSyntax declaration, SyntaxNode root) {
        var memberAccess = (MemberAccessExpressionSyntax)declaration.Expression;
        var identifier = memberAccess.Name;
        var fastIdentifier = SyntaxFactory.IdentifierName(identifier.Identifier + "Fast");

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(identifier, fastIdentifier)));
    }
}
