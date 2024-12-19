using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0001TypesShouldBeSealedAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0001Test {
    [Fact]
    public Task TestTest() => VerifyCS.VerifyAnalyzerAsync("""
        public class [|TestClass|];
        """);
}
