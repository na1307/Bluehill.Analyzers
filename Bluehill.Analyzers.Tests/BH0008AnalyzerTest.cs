using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0008DontRepeatNegatedPatternAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0008AnalyzerTest {
    [Theory]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(object? obj) {
                _ = obj is [|not not|] null;
            }
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(object? obj) {
                _ = obj is [|not not not|] null;
            }
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(TestEnum te) {
                _ = te is [|not not|] TestEnum.One;
            }
        }

        public enum TestEnum {
            One,
            Two,
            Three
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(TestEnum te) {
                _ = te is [|not not not|] TestEnum.One;
            }
        }

        public enum TestEnum {
            One,
            Two,
            Three
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(TestRecord tr) {
                _ = tr is [|not not|] { Id: 0 };
            }
        }

        public readonly record struct TestRecord(int Id, string Name);

        namespace System.Runtime.CompilerServices {
            public sealed class IsExternalInit;
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(TestRecord tr) {
                _ = tr is [|not not not|] { Id: 0 };
            }
        }

        public readonly record struct TestRecord(int Id, string Name);

        namespace System.Runtime.CompilerServices {
            public sealed class IsExternalInit;
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(string str) {
                _ = str is [|not not|] [ 'C', '#' ];
            }
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(string str) {
                _ = str is [|not not not|] [ 'C', '#' ];
            }
        }
        """)]
    public Task Test(string source) => Verify.VerifyAnalyzerAsync(source);
}
