using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0001CodeFixProvider)), Shared]
public sealed class BH0001CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0001ShouldBeSealedAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var declaration = root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
        var action = CodeAction.Create(CodeFixResources.BH0001CodeFixTitle, _ => makeSealedAsync(context.Document, declaration, root), nameof(CodeFixResources.BH0001CodeFixTitle));

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(action, diagnostic);
    }

    private static Task<Document> makeSealedAsync(Document document, TypeDeclarationSyntax typeDecl, SyntaxNode root) {
        var newModifier = typeDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.SealedKeyword));

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(typeDecl, typeDecl.WithModifiers(newModifier))));
    }
}
