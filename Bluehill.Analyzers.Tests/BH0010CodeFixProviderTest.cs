namespace Bluehill.Analyzers.Tests;

public sealed class BH0010CodeFixProviderTest : BHCodeFixProviderTest<BH0010ToBH0011Analyzer, BH0010CodeFixProvider> {
    [Theory]
    [InlineData("""
                using System;

                public class TestClass {
                    public void TestMethod() {
                        var span = "TEST".AsSpan(0, 2);
                        var compare = span {|BH0010:== "TE"|};
                    }
                }
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span is "TE";
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".ToCharArray().AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".ToCharArray().AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".ToCharArray().AsSpan(0, 2);
                             var compare = span is ['T', 'E'];
                         }
                     }
                     """)]
    public Task Test(string source, string fixedSource) => TestStatic(source, fixedSource);
}
