using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeRefactoringVerifier<Bluehill.Analyzers.BR0001,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BR0001Test {
    [Theory]
    [InlineData(
        """
        public class [|THE_TEST_CLASS|];
        """,
        """
        public class TheTestClass;
        """)]
    [InlineData(
        """
        public struct [|THE_TEST_STRUCT|];
        """,
        """
        public struct TheTestStruct;
        """)]
    [InlineData(
        """
        public interface [|THE_TEST_INTERFACE|];
        """,
        """
        public interface TheTestInterface;
        """)]
    [InlineData(
        """
        public record [|THE_TEST_RECORD|];
        """,
        """
        public record TheTestRecord;
        """)]
    [InlineData(
        """
        public record class [|THE_TEST_RECORD|];
        """,
        """
        public record class TheTestRecord;
        """)]
    [InlineData(
        """
        public record struct [|THE_TEST_RECORD|];
        """,
        """
        public record struct TheTestRecord;
        """)]
    [InlineData(
        """
        public enum [|THE_TEST_ENUM|];
        """,
        """
        public enum TheTestEnum;
        """)]
    [InlineData(
        """
        public delegate void [|THE_TEST_DELEGATE|]();
        """,
        """
        public delegate void TheTestDelegate();
        """)]
    [InlineData(
        """
        public class TheTestClass {
            public class [|THE_NESTED_CLASS|];
        }
        """,
        """
        public class TheTestClass {
            public class TheNestedClass;
        }
        """)]
    [InlineData(
        """
        public static class TheTestClass {
            public const int [|THE_INTEGER_VALUE|] = 1000;
        }
        """,
        """
        public static class TheTestClass {
            public const int TheIntegerValue = 1000;
        }
        """)]
    [InlineData(
        """
        public enum TheTestEnum {
            [|THE_ENUM_VALUE|]
        }
        """,
        """
        public enum TheTestEnum {
            TheEnumValue
        }
        """)]
    public Task Test(string source, string fixedSource) => Verify.VerifyRefactoringAsync(source, fixedSource);
}
