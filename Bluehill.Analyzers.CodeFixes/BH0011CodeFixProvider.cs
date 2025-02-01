namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0011CodeFixProvider))]
[Shared]
public sealed class BH0011CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0010ToBH0011Analyzer.DiagnosticIdBH0011];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == BH0010ToBH0011Analyzer.DiagnosticIdBH0011);
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = (BinaryExpressionSyntax)root!.FindToken(diagnosticSpan.Start).Parent!;

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(CodeAction.Create(CodeFixResources.BH0011CodeFixTitle,
            _ => UseSequenceEqualForComparingSpanAndNonConstant(context.Document, node, root),
            nameof(CodeFixResources.BH0011CodeFixTitle)), diagnostic);
    }

    private static Task<Document> UseSequenceEqualForComparingSpanAndNonConstant(Document document, BinaryExpressionSyntax binary, SyntaxNode root) {
        var sequenceEqual = SyntaxFactory.IdentifierName("SequenceEqual");
        var memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, binary.Left, sequenceEqual);
        var argument = SyntaxFactory.Argument(binary.Right);
        var argumentList = SyntaxFactory.ArgumentList([argument]);
        var invocationExpression = SyntaxFactory.InvocationExpression(memberAccessExpression, argumentList);

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(binary, invocationExpression)));
    }
}
