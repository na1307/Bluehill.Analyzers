using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0001TypesShouldBeSealedAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0001AnalyzerTest {
    [Theory]
    [InlineData(
        """
        public class [|TestClass|];
        """)]
    [InlineData(
        """
        public class TestClass;
        public class [|TestClass2|] : TestClass;
        """)]
    [InlineData(
        """
        public class TestClass;
        public sealed class TestClass2 : TestClass;
        """)]
    [InlineData(
        """
        public class [|TestClass|] {
            public class [|NestedClass|];
        }
        """)]
    [InlineData(
        """
        public sealed class TestClass {
            public class [|NestedClass|];
        }
        """)]
    [InlineData(
        """
        public class [|TestClass|] {
            public sealed class NestedClass;
        }
        """)]
    [InlineData(
        """
        public sealed class TestClass {
            public sealed class NestedClass;
        }
        """)]
    [InlineData(
        """
        public sealed class TestClass {
            public class NestedClass;
            public class [|NestedClass2|] : NestedClass;
        }
        """)]
    [InlineData(
        """
        public sealed class TestClass {
            public class NestedClass;
            public sealed class NestedClass2 : NestedClass;
        }
        """)]
    [InlineData(
        """
        public static class TestClass;
        """)]
    [InlineData(
        """
        public sealed class TestClass;
        """)]
    [InlineData(
        """
        public abstract class TestClass;
        """)]
    [InlineData(
        """
        public struct TestStruct;
        """)]
    [InlineData(
        """
        public enum TestEnum;
        """)]
    [InlineData(
        """
        public interface TestInterface;
        """)]
    [InlineData(
        """
        public delegate void TestDelegate();
        """)]
    public Task Test(string source) => Verify.VerifyAnalyzerAsync(source);
}
