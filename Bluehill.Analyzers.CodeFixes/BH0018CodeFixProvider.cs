namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0018CodeFixProvider))]
[Shared]
public sealed class BH0018CodeFixProvider : BHCodeFixProvider<CSharpSyntaxNode> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0018PreferPatternMatchingAnalyzer.DiagnosticId];

    protected override CodeAction CreateCodeAction(Document document, CSharpSyntaxNode syntax, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0018CodeFixTitle, _ => ToPatternMatching(document, syntax, root),
            nameof(CodeFixResources.BH0018CodeFixTitle));

    private static Task<Document> ToPatternMatching(Document document, CSharpSyntaxNode syntax, SyntaxNode root) {
        var binary = (BinaryExpressionSyntax)(syntax is BinaryExpressionSyntax ? syntax : syntax.ChildNodes().Single());
        var isEquals = IsEqualsExpression(binary);
        var value = GetNonNullValue(binary);
        var nullLiteral = SyntaxFactory.ConstantPattern(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
        var isPattern = SyntaxFactory.IsPatternExpression(value, isEquals ? nullLiteral : SyntaxFactory.UnaryPattern(nullLiteral));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(binary, isPattern)));
    }

    private static bool IsEqualsExpression(BinaryExpressionSyntax binary) {
        var equals = binary.IsKind(SyntaxKind.EqualsExpression);
        var notEquals = binary.IsKind(SyntaxKind.NotEqualsExpression);

        if (!equals && !notEquals) {
            throw new InvalidOperationException("Something went wrong");
        }

        return equals && !notEquals;
    }

    private static ExpressionSyntax GetNonNullValue(BinaryExpressionSyntax binary) {
        var leftIsNullLiteral = binary.Left is LiteralExpressionSyntax leftLes && leftLes.IsKind(SyntaxKind.NullLiteralExpression);
        var rightIsNullLiteral = binary.Right is LiteralExpressionSyntax rightLes && rightLes.IsKind(SyntaxKind.NullLiteralExpression);

        if (leftIsNullLiteral) {
            return binary.Right;
        }

        if (rightIsNullLiteral) {
            return binary.Left;
        }

        throw new InvalidOperationException("Something went wrong");
    }
}
