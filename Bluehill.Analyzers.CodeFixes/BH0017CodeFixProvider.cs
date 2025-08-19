namespace Bluehill.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BH0017CodeFixProvider))]
[Shared]
public sealed class BH0017CodeFixProvider : BHCodeFixProvider<IdentifierNameSyntax> {
    public override ImmutableArray<string> FixableDiagnosticIds => [BH0017PreferTypeNameNewAnalyzer.DiagnosticId];

    protected override CodeAction CreateCodeAction(Document document, IdentifierNameSyntax syntax, SyntaxNode root)
        => CodeAction.Create(CodeFixResources.BH0017CodeFixTitle, _ => New(document, syntax, root), nameof(CodeFixResources.BH0017CodeFixTitle));

    private static Task<Document> New(Document document, IdentifierNameSyntax identifier, SyntaxNode root) {
        var variable = (VariableDeclarationSyntax)identifier.Parent!;
        var type = variable.Type;
        var leading = type.GetLeadingTrivia();
        var trailing = type.GetTrailingTrivia();
        var declarator = variable.Variables.Single();
        var initializer = declarator.Initializer!;
        var creation = (ObjectCreationExpressionSyntax)initializer.Value;
        var argumentList = creation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var implicitCreation = SyntaxFactory.ImplicitObjectCreationExpression(argumentList, creation.Initializer);
        var newInitializer = initializer.WithValue(implicitCreation);
        var newDeclarator = declarator.WithInitializer(newInitializer);
        var newType = creation.Type.WithLeadingTrivia(leading).WithTrailingTrivia(trailing);
        var newVariable = variable.WithType(newType).WithVariables([newDeclarator]);

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(variable, newVariable)));
    }
}
