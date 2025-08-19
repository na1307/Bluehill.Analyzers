namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0008CodeFixProvider))]
[Shared]
public sealed class BH0008CodeFixProvider : BHCodeFixProvider<UnaryPatternSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0008DontRepeatNegatedPatternAnalyzer.DiagnosticId];

    protected override CodeAction CreateCodeAction(Document document, UnaryPatternSyntax syntax, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0008CodeFixTitle, _ => RemoveRepeatedNegatedPatterns(document, syntax, root),
            nameof(CodeFixResources.BH0008CodeFixTitle));

    private static Task<Document> RemoveRepeatedNegatedPatterns(Document document, UnaryPatternSyntax notPattern, SyntaxNode root) {
        var parent = (ExpressionOrPatternSyntax)notPattern.Parent!;
        var notPatterns = notPattern.DescendantNodesAndSelf().OfType<UnaryPatternSyntax>().ToArray();

        // Even count of 'not' patterns: Remove all 'not'
        // Odd count of 'not' patterns: Leave only one 'not'
        var realPattern = notPatterns.Length % 2 == 0 ? notPatterns[notPatterns.Length - 1].Pattern : notPatterns[notPatterns.Length - 1];
        var newParent = parent.ReplaceNode(notPattern, realPattern);
        var newRoot = root.ReplaceNode(parent, newParent);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
