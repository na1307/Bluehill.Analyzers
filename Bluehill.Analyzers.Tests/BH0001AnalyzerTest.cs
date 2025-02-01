namespace Bluehill.Analyzers.Tests;

public sealed class BH0001AnalyzerTest : BHAnalyzerTest<BH0001TypesShouldBeSealedAnalyzer> {
    [Theory]
    [InlineData("""
                public class [|TestClass|];
                """)]
    [InlineData("""
                public class TestClass;
                public class [|TestClass2|] : TestClass;
                """)]
    [InlineData("""
                public class TestClass;
                public sealed class TestClass2 : TestClass;
                """)]
    [InlineData("""
                public class TestClass;
                public class TestClass2 : TestClass;
                public class [|TestClass3|] : TestClass2;
                """)]
    [InlineData("""
                public class TestClass;
                public class TestClass2 : TestClass;
                public sealed class TestClass3 : TestClass2;
                """)]
    [InlineData("""
                public class [|TestClass|] {
                    public class [|NestedClass|];
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public class [|NestedClass|];
                }
                """)]
    [InlineData("""
                public class [|TestClass|] {
                    public sealed class NestedClass;
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public sealed class NestedClass;
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public class NestedClass;
                    public class [|NestedClass2|] : NestedClass;
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public class NestedClass;
                    public sealed class NestedClass2 : NestedClass;
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public class NestedClass;
                    public class NestedClass2 : NestedClass;
                    public class [|NestedClass3|] : NestedClass2;
                }
                """)]
    [InlineData("""
                public sealed class TestClass {
                    public class NestedClass;
                    public class NestedClass2 : NestedClass;
                    public sealed class NestedClass3 : NestedClass2;
                }
                """)]
    [InlineData("""
                public static class TestClass;
                """)]
    [InlineData("""
                public sealed class TestClass;
                """)]
    [InlineData("""
                public abstract class TestClass;
                """)]
    [InlineData("""
                public struct TestStruct;
                """)]
    [InlineData("""
                public enum TestEnum;
                """)]
    [InlineData("""
                public interface TestInterface;
                """)]
    [InlineData("""
                public delegate void TestDelegate();
                """)]
    public Task Test(string source) => TestStatic(source);
}
