using System.Text.RegularExpressions;

namespace Bluehill.Analyzers;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(BR0001ConvertFromSnakeCaseToPascalCaseRefactor))]
[Shared]
public sealed class BR0001ConvertFromSnakeCaseToPascalCaseRefactor : CodeRefactoringProvider {
    private const string Cgsr = "Couldn't got syntax root";
    private static readonly Regex ScreamingSnakeCaseRegex = new("^[A-Z0-9_]+$");

    public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
        // Get the document and syntax root
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken);
        var span = context.Span;

        // Find the node at the specified span
        var node = root?.FindToken(span.Start).Parent;

        // Check is the node is a type
        if (node is BaseTypeDeclarationSyntax type) {
            HandleType(context, document, type);
        }

        // Check if the node is a constant field
        if (node is VariableDeclaratorSyntax variable &&
            variable.Parent is VariableDeclarationSyntax declaration &&
            declaration.Parent is FieldDeclarationSyntax field &&
            field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword))) {
            HandleConstant(context, document, variable);
        }

        // Check if the node is an enum member
        if (node is EnumMemberDeclarationSyntax enumMember) {
            HandleEnumMember(context, document, enumMember);
        }

        // Check if the node is a delegate
        if (node is DelegateDeclarationSyntax @delegate) {
            HandleDelegate(context, document, @delegate);
        }
    }

    private static void HandleType(CodeRefactoringContext context, Document document, BaseTypeDeclarationSyntax type) {
        var identifier = type.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create("Convert type to Pascal Case",
                ct => ConvertTypeToPascalCaseAsync(document, type, identifier, ct), equivalenceKey: "ConvertTypeToPascalCase"));
        }
    }

    private static void HandleConstant(
        CodeRefactoringContext context,
        Document document,
        VariableDeclaratorSyntax variable) {
        var identifier = variable.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create("Convert constant to Pascal Case",
                ct => ConvertConstantToPascalCaseAsync(document, variable, identifier, ct), equivalenceKey: "ConvertConstantToPascalCase"));
        }
    }

    private static void HandleEnumMember(
        CodeRefactoringContext context,
        Document document,
        EnumMemberDeclarationSyntax enumMember) {
        var identifier = enumMember.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create("Convert enum member to Pascal Case",
                ct => ConvertEnumMemberToPascalCaseAsync(document, enumMember, identifier, ct), equivalenceKey: "ConvertEnumMemberToPascalCase"));
        }
    }

    private static void HandleDelegate(
        CodeRefactoringContext context,
        Document document,
        DelegateDeclarationSyntax @delegate) {
        var identifier = @delegate.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create("Convert delegate to Pascal Case",
                ct => ConvertDelegateToPascalCaseAsync(document, @delegate, identifier, ct), equivalenceKey: "ConvertDelegateToPascalCase"));
        }
    }

    private static async Task<Document> ConvertTypeToPascalCaseAsync(
        Document document,
        BaseTypeDeclarationSyntax type,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = ToPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException(Cgsr);
        var newType = type.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(type, newType);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ConvertConstantToPascalCaseAsync(
        Document document,
        VariableDeclaratorSyntax variable,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = ToPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException(Cgsr);
        var newVariable = variable.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(variable, newVariable);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ConvertEnumMemberToPascalCaseAsync(
        Document document,
        EnumMemberDeclarationSyntax enumMember,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = ToPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException(Cgsr);
        var newEnumMember = enumMember.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(enumMember, newEnumMember);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> ConvertDelegateToPascalCaseAsync(
        Document document,
        DelegateDeclarationSyntax @delegate,
        string identifier,
        CancellationToken cancellationToken) {
        var pascalCaseName = ToPascalCase(identifier);

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException(Cgsr);
        var newDelegate = @delegate.WithIdentifier(SyntaxFactory.Identifier(pascalCaseName));
        var newRoot = root.ReplaceNode(@delegate, newDelegate);

        return document.WithSyntaxRoot(newRoot);
    }

    private static bool IsScreamingSnakeCase(string identifier) => ScreamingSnakeCaseRegex.IsMatch(identifier) && identifier.Contains("_");

    private static string ToPascalCase(string screamingSnake)
        => string.Concat(screamingSnake.Split(['_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
}
