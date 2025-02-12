namespace Bluehill.Analyzers;

public abstract class BHCodeFixProvider<TNode> : CodeFixProvider where TNode : CSharpSyntaxNode {
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == FixableDiagnosticIds.Single());
        var root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;
        var declaration = (TNode)root.FindNode(diagnostic.Location.SourceSpan);
        var action = CreateCodeAction(context.Document, declaration, root);

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(action, diagnostic);
    }

    protected abstract CodeAction CreateCodeAction(Document document, TNode declaration, SyntaxNode root);
}
