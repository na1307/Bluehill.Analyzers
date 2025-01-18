namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0008CodeFixProvider))]
[Shared]
public sealed class BH0008CodeFixProvider : CodeFixProvider {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0008DontRepeatNegatedPatternAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        var diagnostic = context.Diagnostics.Single(d => d.Id == "BH0008");
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var node = (UnaryPatternSyntax)root!.FindToken(diagnosticSpan.Start).Parent!;

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(CodeFixResources.BH0008CodeFixTitle,
                _ => RemoveRepeatedNegatedPatterns(context.Document, node, root),
                nameof(CodeFixResources.BH0008CodeFixTitle)), diagnostic);
    }

    private static Task<Document> RemoveRepeatedNegatedPatterns(Document document, UnaryPatternSyntax notPattern,
        SyntaxNode root) {
        var parent = (ExpressionOrPatternSyntax)notPattern.Parent!;
        var notPatterns = notPattern.DescendantNodesAndSelf().OfType<UnaryPatternSyntax>().ToArray();

        // Even count of 'not' patterns: Remove all 'not'
        // Odd count of 'not' patterns: Leave only one 'not'
        var realPattern = notPatterns.Length % 2 == 0
            ? notPatterns[notPatterns.Length - 1].Pattern
            : notPatterns[notPatterns.Length - 1];

        var newParent = parent.ReplaceNode(notPattern, realPattern);
        var newRoot = root.ReplaceNode(parent, newParent);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
