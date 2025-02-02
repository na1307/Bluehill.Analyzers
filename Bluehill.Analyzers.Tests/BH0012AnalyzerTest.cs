namespace Bluehill.Analyzers.Tests;

public sealed class BH0012AnalyzerTest : BHAnalyzerTest<BH0012FieldNameShouldNotConflictAnalyzer> {
    [Theory]
    [InlineData("""
                public class TestClass(int id) {
                    private readonly int [|id|];
                }
                """)]
    public Task Test(string source) => TestStatic(source);
}
