using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Bluehill.Analyzers.BH0003ProhibitPcpReassignmentAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0003AnalyzerTest {
    [Theory]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        [|i|] = 10;
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        ([|i|], var i2) = new int[] { 10, 20 };
                    }
                }

                public static class IntArrayExtensions {
                    public static void Deconstruct(this int[] array, out int i1, out int i2) {
                        i1 = array[0];
                        i2 = array[1];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        [|i|] += 10;
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        [|i|] -= 10;
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(string? s) {
                    public void TestMethod() {
                        [|s|] ??= string.Empty;
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        [|i|]++;
                    }
                }
                """)]
    [InlineData("""
                public class TestClass(int i) {
                    public void TestMethod() {
                        [|i|]--;
                    }
                }
                """)]
    public Task Test(string source) => Verify.VerifyAnalyzerAsync(source.ReplaceLineEndings());
}
