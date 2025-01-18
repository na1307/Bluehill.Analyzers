using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0002FieldsShouldBeAtTheTopAnalyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0002AnalyzerTest {
    [Theory]
    [InlineData(
        """
        public class TestClass {
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        
            private readonly string [|name|];
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            private readonly string name;
        
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        
            [System.NonSerialized]
            private readonly string [|name|];
        }
        """)]
    [InlineData(
        """
        public class TestClass {
            [System.NonSerialized]
            private readonly string name;
        
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        }
        """)]
    [InlineData(
        """
        public partial class TestClass {
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        
            private readonly string name;
        }
        """)]
    [InlineData(
        """
        public record class TestClass {
            public TestClass(string name) => this.name = name;
        
            public string GetName() => name;
        
            private readonly string name;
        }
        """)]
    [InlineData(
        """
        public class TestClass(string constructorName) {
            public string GetName() => name;
        
            private readonly string [|name|] = constructorName;
        }
        """)]
    [InlineData(
        """
        public class TestClass(string constructorName) {
            private readonly string name = constructorName;
        
            public string GetName() => name;
        }
        """)]
    public Task Test(string source) => Verify.VerifyAnalyzerAsync(source);
}
