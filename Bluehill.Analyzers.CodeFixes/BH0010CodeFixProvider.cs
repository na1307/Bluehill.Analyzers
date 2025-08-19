using Roslynator.CSharp;

namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0010CodeFixProvider))]
[Shared]
public sealed class BH0010CodeFixProvider : BHCodeFixProvider<BinaryExpressionSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0010ToBH0011Analyzer.DiagnosticIdBH0010];

    protected override CodeAction CreateCodeAction(Document document, BinaryExpressionSyntax syntax, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0010CodeFixTitle, _ => UsePatternMatchingForComparingSpanAndConstant(document, syntax, root),
            nameof(CodeFixResources.BH0010CodeFixTitle));

    private static Task<Document> UsePatternMatchingForComparingSpanAndConstant(Document document, BinaryExpressionSyntax binary, SyntaxNode root) {
        var isPatternExpression = binary.Right.Kind() switch {
            SyntaxKind.StringLiteralExpression => ProcessStringLiteral(binary),
            SyntaxKind.CollectionExpression => ProcessCollection(binary),
            SyntaxKind.ArrayCreationExpression => ProcessArrayCreation(binary),
            SyntaxKind.ImplicitArrayCreationExpression => ProcessImplicitArrayCreation(binary),
            _ => throw new InvalidOperationException("Something went wrong")
        };

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(binary, isPatternExpression)));
    }

    private static IsPatternExpressionSyntax ProcessStringLiteral(BinaryExpressionSyntax binary) {
        var constantPattern = SyntaxFactory.ConstantPattern(binary.Right);

        return SyntaxFactory.IsPatternExpression(binary.Left, constantPattern);
    }

    private static IsPatternExpressionSyntax ProcessCollection(BinaryExpressionSyntax binary) {
        var c = (CollectionExpressionSyntax)binary.Right;

        var listPattern = SyntaxFactory.ListPattern(c.Elements.Cast<ExpressionElementSyntax>()
            .Select(e => SyntaxFactory.ConstantPattern(e.Expression)).Cast<PatternSyntax>().ToSeparatedSyntaxList());

        return SyntaxFactory.IsPatternExpression(binary.Left, listPattern);
    }

    private static IsPatternExpressionSyntax ProcessArrayCreation(BinaryExpressionSyntax binary) {
        var ac = (ArrayCreationExpressionSyntax)binary.Right;

        var listPattern = SyntaxFactory.ListPattern(ac.Initializer?.Expressions.Select(SyntaxFactory.ConstantPattern).Cast<PatternSyntax>()
            .ToSeparatedSyntaxList() ?? []);

        return SyntaxFactory.IsPatternExpression(binary.Left, listPattern);
    }

    private static IsPatternExpressionSyntax ProcessImplicitArrayCreation(BinaryExpressionSyntax binary) {
        var iac = (ImplicitArrayCreationExpressionSyntax)binary.Right;

        var listPattern = SyntaxFactory.ListPattern(iac.Initializer.Expressions.Select(SyntaxFactory.ConstantPattern).Cast<PatternSyntax>()
            .ToSeparatedSyntaxList());

        return SyntaxFactory.IsPatternExpression(binary.Left, listPattern);
    }
}
