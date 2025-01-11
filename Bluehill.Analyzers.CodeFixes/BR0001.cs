using System.Text.RegularExpressions;

namespace Bluehill.Analyzers;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(BR0001))]
[Shared]
internal sealed class BR0001 : CodeRefactoringProvider {
    private static readonly Regex screamingSnakeCaseRegex = new("^[A-Z0-9_]+$");

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
        // Get the document and syntax root
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken);
        var span = context.Span;

        // Find the node at the specified span
        var node = root?.FindToken(span.Start).Parent;

        // Check is the node is a type
        if (node is BaseTypeDeclarationSyntax type) {
            handleType(context, document, type);
        }

        // Check if the node is a constant field
        if (node is VariableDeclaratorSyntax variable &&
            variable.Parent is VariableDeclarationSyntax declaration &&
            declaration.Parent is FieldDeclarationSyntax field &&
            field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword))) {
            handleConstant(context, document, variable);
        }

        // Check if the node is an enum member
        if (node is EnumMemberDeclarationSyntax enumMember) {
            handleEnumMember(context, document, enumMember);
        }

        // Check if the node is a delegate
        if (node is DelegateDeclarationSyntax @delegate) {
            handleDelegate(context, document, @delegate);
        }
    }

    private static void handleType(CodeRefactoringContext context, Document document, BaseTypeDeclarationSyntax type) {
        var identifier = type.Identifier.Text;

        if (isScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Convert type to Pascal Case",
                    ct => convertTypeToPascalCaseAsync(document, type, identifier, ct),
                    equivalenceKey: "ConvertTypeToPascalCase"));
        }
    }

    private static void handleConstant(CodeRefactoringContext context, Document document, VariableDeclaratorSyntax variable) {
        var identifier = variable.Identifier.Text;

        if (isScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Convert constant to Pascal Case",
                    ct => convertConstantToPascalCaseAsync(document, variable, identifier, ct),
                    equivalenceKey: "ConvertConstantToPascalCase"));
        }
    }

    private static void handleEnumMember(CodeRefactoringContext context, Document document, EnumMemberDeclarationSyntax enumMember) {
        var identifier = enumMember.Identifier.Text;

        if (isScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Convert enum member to Pascal Case",
                    ct => convertEnumMemberToPascalCaseAsync(document, enumMember, identifier, ct),
                    equivalenceKey: "ConvertEnumMemberToPascalCase"));
        }
    }

    private static void handleDelegate(CodeRefactoringContext context, Document document, DelegateDeclarationSyntax @delegate) {
        var identifier = @delegate.Identifier.Text;

        if (isScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(
                CodeAction.Create(
                    "Convert delegate to Pascal Case",
                    ct => convertDelegateToPascalCaseAsync(document, @delegate, identifier, ct),
                    equivalenceKey: "ConvertDelegateToPascalCase"));
        }
    }

    private static async Task<Document> convertTypeToPascalCaseAsync(
        Document document,
        BaseTypeDeclarationSyntax type,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = toPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException("Couldn't got syntax root");
        var newType = type.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(type, newType);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> convertConstantToPascalCaseAsync(
        Document document,
        VariableDeclaratorSyntax variable,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = toPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException("Couldn't got syntax root");
        var newVariable = variable.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(variable, newVariable);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> convertEnumMemberToPascalCaseAsync(
        Document document,
        EnumMemberDeclarationSyntax enumMember,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = toPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException("Couldn't got syntax root");
        var newEnumMember = enumMember.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(enumMember, newEnumMember);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> convertDelegateToPascalCaseAsync(
    Document document,
    DelegateDeclarationSyntax @delegate,
    string identifier,
    CancellationToken cancellationToken) {
        var pascalCaseName = toPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException("Couldn't got syntax root");
        var newDelegate = @delegate.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(@delegate, newDelegate);

        return document.WithSyntaxRoot(newRoot);
    }

    private static bool isScreamingSnakeCase(string identifier) => screamingSnakeCaseRegex.IsMatch(identifier) && identifier.Contains("_");

    private static string toPascalCase(string screamingSnake) => string.Concat(screamingSnake
            .Split(['_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
}
