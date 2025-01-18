using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Bluehill.Analyzers.BH0008DontRepeatNegatedPatternAnalyzer,
    Bluehill.Analyzers.BH0008CodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0008CodeFixProviderTest {
    [Theory]
    [InlineData(
        """
        public class TestClass {
            public void TestMethod(object? obj) {
                _ = obj is [|not not|] null;
            }
        }
        """,
        """
        public class TestClass {
            public void TestMethod(object? obj) {
                _ = obj is null;
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
        """,
        """
        public class TestClass {
            public void TestMethod(object? obj) {
                _ = obj is not null;
            }
        }
        """)]
#pragma warning disable SA1005, S125
    // https://github.com/dotnet/roslyn-sdk/issues/1202
    //[InlineData(
    //    """
    //    public class TestClass {
    //        public void TestMethod(TestEnum te) {
    //            _ = te is [|not not|] TestEnum.One;
    //        }
    //    }

    //    public enum TestEnum {
    //        One,
    //        Two,
    //        Three
    //    }
    //    """, """
    //    public class TestClass {
    //        public void TestMethod(TestEnum te) {
    //            _ = te is TestEnum.One;
    //        }
    //    }

    //    public enum TestEnum {
    //        One,
    //        Two,
    //        Three
    //    }
    //    """
    //    )]
#pragma warning restore SA1005, S125
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
        """,
        """
        public class TestClass {
            public void TestMethod(TestEnum te) {
                _ = te is not TestEnum.One;
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
        """,
        """
        public class TestClass {
            public void TestMethod(TestRecord tr) {
                _ = tr is { Id: 0 };
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
        """,
        """
        public class TestClass {
            public void TestMethod(TestRecord tr) {
                _ = tr is not { Id: 0 };
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
        """,
        """
        public class TestClass {
            public void TestMethod(string str) {
                _ = str is [ 'C', '#' ];
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
        """,
        """
        public class TestClass {
            public void TestMethod(string str) {
                _ = str is not [ 'C', '#' ];
            }
        }
        """)]
    public Task Test(string source, string fixedSource) => Verify.VerifyCodeFixAsync(source, fixedSource);
}
