namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0001CodeFixProvider))]
[Shared]
public sealed class BH0001CodeFixProvider : BHCodeFixProvider<TypeDeclarationSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0001TypesShouldBeSealedAnalyzer.DiagnosticId];

    protected override CodeAction CreateCodeAction(Document document, TypeDeclarationSyntax declaration, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0001CodeFixTitle, _ => MakeSealedAsync(document, declaration, root),
            nameof(CodeFixResources.BH0001CodeFixTitle));

    private static Task<Document> MakeSealedAsync(Document document, TypeDeclarationSyntax typeDecl, SyntaxNode root) {
        var newModifier = typeDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(typeDecl, typeDecl.WithModifiers(newModifier))));
    }
}
