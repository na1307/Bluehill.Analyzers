namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0005CodeFixProvider))]
[Shared]
public sealed class BH0005CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0004ToBH0006Analyzer.DiagnosticIdBH0005];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == BH0004ToBH0006Analyzer.DiagnosticIdBH0005);
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = root!.FindToken(diagnosticSpan.Start).Parent!;
        var action = CodeAction.Create(CodeFixResources.BH0005CodeFixTitle, _ => ProcessAsync(context.Document, node, root), nameof(CodeFixResources.BH0005CodeFixTitle));

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(action, diagnostic);
    }

    private static Task<Document> ProcessAsync(Document document, SyntaxNode node, SyntaxNode root)
        => Task.FromResult(document.WithSyntaxRoot(node switch {
            MethodDeclarationSyntax methodDeclaration => ProcessAbstract(methodDeclaration, root),
            BlockSyntax block => ProcessBlock(block, root),
            ArrowExpressionClauseSyntax arrow => ProcessArrow(arrow, root),
            _ => throw new InvalidOperationException("Something went wrong"),
        }));

    private static SyntaxNode ProcessAbstract(
        MethodDeclarationSyntax methodDeclaration,
        SyntaxNode root) {
        // Remove `abstract` modifier
        var modifiers = methodDeclaration.Modifiers;
        var abstractKeyword = modifiers.Single(t => t.IsKind(SyntaxKind.AbstractKeyword));
        var newModifiers = modifiers.Remove(abstractKeyword);

        // Create a new method declaration
        var newDeclaration = methodDeclaration.WithModifiers(newModifiers)
            .WithExpressionBody(CreateNullLiteralArrowExpression()).WithLeadingTrivia(methodDeclaration.GetLeadingTrivia())
            .WithTrailingTrivia(methodDeclaration.GetTrailingTrivia());

        // Replace the old method declaration with the new one in the syntax tree
        return root.ReplaceNode(methodDeclaration, newDeclaration);
    }

    private static SyntaxNode ProcessBlock(BlockSyntax block, SyntaxNode root) {
        // Get method declaration
        var methodDeclaration = (MethodDeclarationSyntax)block.Parent!;

        // New method declaration
        var newDeclaration = methodDeclaration.WithBody(null).WithExpressionBody(CreateNullLiteralArrowExpression())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(methodDeclaration.GetLeadingTrivia())
            .WithTrailingTrivia(methodDeclaration.GetTrailingTrivia());

        // Replace the old method declaration with the new one in the syntax tree
        return root.ReplaceNode(methodDeclaration, newDeclaration);
    }

    private static SyntaxNode ProcessArrow(ArrowExpressionClauseSyntax arrow, SyntaxNode root) => root.ReplaceNode(arrow, CreateNullLiteralArrowExpression());

    private static ArrowExpressionClauseSyntax CreateNullLiteralArrowExpression() {
        // Create an Arrow expression that return null
        var nullLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

        return SyntaxFactory.ArrowExpressionClause(nullLiteral);
    }
}
