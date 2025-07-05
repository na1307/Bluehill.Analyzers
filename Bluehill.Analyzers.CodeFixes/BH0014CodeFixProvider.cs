namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0014CodeFixProvider))]
[Shared]
public sealed class BH0014CodeFixProvider : BHCodeFixProvider<CSharpSyntaxNode> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0014ToBH0015Analyzer.DiagnosticIdBH0014];

    protected override CodeAction CreateCodeAction(Document document, CSharpSyntaxNode declaration, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0014CodeFixTitle, _ => SuffixFast(document, declaration, root),
        nameof(CodeFixResources.BH0014CodeFixTitle));

    private static Task<Document> SuffixFast(Document document, CSharpSyntaxNode declaration, SyntaxNode root) {
        var memberAccess = (MemberAccessExpressionSyntax)(declaration switch {
            ArgumentSyntax argst => ((InvocationExpressionSyntax)argst.Expression).Expression,
            InvocationExpressionSyntax ies => ies.Expression,
            _ => throw new NotSupportedException()
        });

        var identifier = memberAccess.Name;
        var fastIdentifier = SyntaxFactory.IdentifierName(identifier.Identifier + "Fast");

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(identifier, fastIdentifier)));
    }
}
