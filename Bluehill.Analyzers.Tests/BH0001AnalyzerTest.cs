using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0001TypesShouldBeSealedAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0001AnalyzerTest {
    [Fact]
    public Task NonSealedClass() => Verify.VerifyAnalyzerAsync("""
        public class [|TestClass|];
        """);

    [Fact]
    public Task NonSealedAndNonSealedDerivedClass() => Verify.VerifyAnalyzerAsync("""
        public class TestClass;
        public class [|TestClass2|] : TestClass;
        """);

    [Fact]
    public Task NonSealedAndSealedDerivedClass() => Verify.VerifyAnalyzerAsync("""
        public class TestClass;
        public sealed class TestClass2 : TestClass;
        """);

    [Fact]
    public Task StaticClass() => Verify.VerifyAnalyzerAsync("""
        public static class TestClass;
        """);

    [Fact]
    public Task SealedClassWithoutBase() => Verify.VerifyAnalyzerAsync("""
        public sealed class TestClass;
        """);

    [Fact]
    public Task AbstractClassWithoutBase() => Verify.VerifyAnalyzerAsync("""
        public abstract class TestClass;
        """);

    [Fact]
    public Task Struct() => Verify.VerifyAnalyzerAsync("""
        public struct TestStruct;
        """);

    [Fact]
    public Task Enum() => Verify.VerifyAnalyzerAsync("""
        public enum TestEnum;
        """);

    [Fact]
    public Task Interface() => Verify.VerifyAnalyzerAsync("""
        public interface TestInterface;
        """);

    [Fact]
    public Task Delegate() => Verify.VerifyAnalyzerAsync("""
        public delegate void TestDelegate();
        """);
}
