namespace Bluehill.Analyzers.Tests;

public sealed class BH0010ToBH0011AnalyzerTest : BHAnalyzerTest<BH0010ToBH0011Analyzer> {
    [Theory]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0010:== "TE"|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0010:== ['T', 'E']|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".ToCharArray().AsSpan(0, 2);
                        var compare = span {|BH0010:== ['T', 'E']|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0010:== new char[] { 'T', 'E' }|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".ToCharArray().AsSpan(0, 2);
                        var compare = span {|BH0010:== new char[] { 'T', 'E' }|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0010:== new[] { 'T', 'E' }|};
                    }
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".ToCharArray().AsSpan(0, 2);
                        var compare = span {|BH0010:== new[] { 'T', 'E' }|};
                    }
                }
                """)]
    [InlineData("""
            using System;

            public class TestClass {
                public void TestMethod() {
                    var span = "TEST".AsSpan(0, 2);
                    var chr = 'T';
                    char[] str = [chr, 'E'];
                    var compare = span {|BH0011:== str|};
                }
            }
            """)]
    [InlineData("""
            using System;

            public class TestClass {
                public void TestMethod() {
                    var span = "TEST".ToCharArray().AsSpan(0, 2);
                    var chr = 'T';
                    char[] str = [chr, 'E'];
                    var compare = span {|BH0011:== str|};
                }
            }
            """)]
    [InlineData("""
            using System;

            public class TestClass {
                public void TestMethod() {
                    var span = "TEST".AsSpan(0, 2);
                    char[] str = ['T', 'E'];
                    var compare = span {|BH0011:== str|};
                }
            }
            """)]
    [InlineData("""
            using System;

            public class TestClass {
                public void TestMethod() {
                    var span = "TEST".ToCharArray().AsSpan(0, 2);
                    char[] str = ['T', 'E'];
                    var compare = span {|BH0011:== str|};
                }
            }
            """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0011:== GetStr()|};
                    }

                    public char[] GetStr() => ['T', 'E'];
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".ToCharArray().AsSpan(0, 2);
                        var compare = span {|BH0011:== GetStr()|};
                    }

                    public char[] GetStr() => ['T', 'E'];
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0011:== Str|};
                    }

                    public char[] Str => ['T', 'E'];
                }
                """)]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".ToCharArray().AsSpan(0, 2);
                        var compare = span {|BH0011:== Str|};
                    }

                    public char[] Str => ['T', 'E'];
                }
                """)]
    public Task Test(string source) => TestStatic(source);
}
