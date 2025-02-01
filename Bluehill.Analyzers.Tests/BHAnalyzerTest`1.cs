using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Bluehill.Analyzers.Tests;

public abstract class BHAnalyzerTest<TAnalyzer> where TAnalyzer : BHAnalyzer, new() {
    protected static Task TestStatic(string source) => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.VerifyAnalyzerAsync(source.ReplaceLineEndings());
}
