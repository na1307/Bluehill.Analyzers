namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0011CodeFixProvider))]
[Shared]
public sealed class BH0011CodeFixProvider : BHCodeFixProvider<BinaryExpressionSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0010ToBH0011Analyzer.DiagnosticIdBH0011];

    protected override CodeAction CreateCodeAction(Document document, BinaryExpressionSyntax syntax, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0011CodeFixTitle, _ => UseSequenceEqualForComparingSpanAndNonConstant(document, syntax, root),
            nameof(CodeFixResources.BH0011CodeFixTitle));

    private static Task<Document> UseSequenceEqualForComparingSpanAndNonConstant(Document document, BinaryExpressionSyntax binary, SyntaxNode root) {
        var sequenceEqual = SyntaxFactory.IdentifierName("SequenceEqual");
        var memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, binary.Left, sequenceEqual);
        var argument = SyntaxFactory.Argument(binary.Right);
        var argumentList = SyntaxFactory.ArgumentList([argument]);
        var invocationExpression = SyntaxFactory.InvocationExpression(memberAccessExpression, argumentList);

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(binary, invocationExpression)));
    }
}
