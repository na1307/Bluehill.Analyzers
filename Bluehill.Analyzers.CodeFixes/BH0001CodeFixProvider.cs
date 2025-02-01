namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0001CodeFixProvider))]
[Shared]
public sealed class BH0001CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0001TypesShouldBeSealedAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == BH0001TypesShouldBeSealedAnalyzer.DiagnosticId);
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var declaration = root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf().OfType<TypeDeclarationSyntax>()
            .First();

        var action = CodeAction.Create(CodeFixResources.BH0001CodeFixTitle, _ => MakeSealedAsync(context.Document, declaration, root),
            nameof(CodeFixResources.BH0001CodeFixTitle));

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(action, diagnostic);
    }

    private static Task<Document> MakeSealedAsync(Document document, TypeDeclarationSyntax typeDecl, SyntaxNode root) {
        var newModifier = typeDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(typeDecl, typeDecl.WithModifiers(newModifier))));
    }
}
