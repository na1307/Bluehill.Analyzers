using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Bluehill.Analyzers.Tests;

public abstract class BHCodeFixProviderTest<TAnalyzer, TCodeFixProvider>
    where TAnalyzer : BHAnalyzer, new()
    where TCodeFixProvider : CodeFixProvider, new() {
    protected static Task TestStatic(string source, string fixedSource) => CSharpCodeFixVerifier<TAnalyzer, TCodeFixProvider, DefaultVerifier>
        .VerifyCodeFixAsync(source.ReplaceLineEndings(), fixedSource.ReplaceLineEndings());
}
