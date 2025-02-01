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

        switch (root?.FindToken(context.Span.Start).Parent) {
            // Check is the node is a type
            case BaseTypeDeclarationSyntax type:
                HandleType(context, document, type);
                break;

            // Check if the node is a constant field
            case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax field } } variable when
                field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)):
                HandleConstant(context, document, variable);
                break;

            // Check if the node is an enum member
            case EnumMemberDeclarationSyntax enumMember:
                HandleEnumMember(context, document, enumMember);
                break;

            // Check if the node is a delegate
            case DelegateDeclarationSyntax @delegate:
                HandleDelegate(context, document, @delegate);
                break;
        }
    }

    private static void HandleType(CodeRefactoringContext context, Document document, BaseTypeDeclarationSyntax type) {
        var identifier = type.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create(CodeFixResources.BR0001Refactortitle,
                ct => ConvertTypeToPascalCaseAsync(document, type, identifier, ct), equivalenceKey: "ConvertTypeToPascalCase"));
        }
    }

    private static void HandleConstant(
        CodeRefactoringContext context,
        Document document,
        VariableDeclaratorSyntax variable) {
        var identifier = variable.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create(CodeFixResources.BR0001Refactortitle,
                ct => ConvertConstantToPascalCaseAsync(document, variable, identifier, ct), equivalenceKey: "ConvertConstantToPascalCase"));
        }
    }

    private static void HandleEnumMember(
        CodeRefactoringContext context,
        Document document,
        EnumMemberDeclarationSyntax enumMember) {
        var identifier = enumMember.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create(CodeFixResources.BR0001Refactortitle,
                ct => ConvertEnumMemberToPascalCaseAsync(document, enumMember, identifier, ct), equivalenceKey: "ConvertEnumMemberToPascalCase"));
        }
    }

    private static void HandleDelegate(
        CodeRefactoringContext context,
        Document document,
        DelegateDeclarationSyntax @delegate) {
        var identifier = @delegate.Identifier.Text;

        if (IsScreamingSnakeCase(identifier)) {
            context.RegisterRefactoring(CodeAction.Create(CodeFixResources.BR0001Refactortitle,
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

    private static string ToPascalCase(string screamingSnake) => string.Concat(screamingSnake.Split(['_'], StringSplitOptions.RemoveEmptyEntries)
        .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
}
