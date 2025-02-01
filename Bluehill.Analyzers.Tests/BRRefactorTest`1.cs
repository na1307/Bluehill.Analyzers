using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Bluehill.Analyzers.Tests;

public abstract class BRRefactorTest<TRefactor> where TRefactor : CodeRefactoringProvider, new() {
    protected static Task TestStatic(string source, string fixedSource) => CSharpCodeRefactoringVerifier<TRefactor, DefaultVerifier>
        .VerifyRefactoringAsync(source.ReplaceLineEndings(), fixedSource.ReplaceLineEndings());
}
