using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<Bluehill.Analyzers.BH0001TypesShouldBeSealedAnalyzer,
        Bluehill.Analyzers.BH0001CodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0001CodeFixProviderTest {
    [Theory]
    [InlineData(
        """
        public class [|TestClass|];
        """,
        """
        public sealed class TestClass;
        """
    )]
    [InlineData(
        """
        public class TestClass;
        public class [|TestClass2|] : TestClass;
        """,
        """
        public class TestClass;
        public sealed class TestClass2 : TestClass;
        """
    )]
    [InlineData(
        """
        public class [|TestClass|] {
            public class [|NestedClass|];
        }
        """,
        """
        public sealed class TestClass {
            public sealed class NestedClass;
        }
        """
    )]
    [InlineData(
        """
        public sealed class TestClass {
            public class [|NestedClass|];
        }
        """,
        """
        public sealed class TestClass {
            public sealed class NestedClass;
        }
        """
    )]
    [InlineData(
        """
        public class [|TestClass|] {
            public sealed class NestedClass;
        }
        """,
        """
        public sealed class TestClass {
            public sealed class NestedClass;
        }
        """
    )]
    [InlineData(
        """
        public sealed class TestClass {
            public class NestedClass;
            public class [|NestedClass2|] : NestedClass;
        }
        """,
        """
        public sealed class TestClass {
            public class NestedClass;
            public sealed class NestedClass2 : NestedClass;
        }
        """
    )]
    public Task Test(string source, string fixedSource) => Verify.VerifyCodeFixAsync(source, fixedSource);
}
