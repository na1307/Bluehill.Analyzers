namespace Bluehill.Analyzers.Tests;

public sealed class BH0011CodeFixProviderTest : BHCodeFixProviderTest<BH0010ToBH0011Analyzer, BH0011CodeFixProvider> {
    [Theory]
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
            """, """
                 using System;

                 public class TestClass {
                     public void TestMethod() {
                         var span = "TEST".AsSpan(0, 2);
                         var chr = 'T';
                         char[] str = [chr, 'E'];
                         var compare = span.SequenceEqual(str);
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
            """, """
                 using System;

                 public class TestClass {
                     public void TestMethod() {
                         var span = "TEST".ToCharArray().AsSpan(0, 2);
                         var chr = 'T';
                         char[] str = [chr, 'E'];
                         var compare = span.SequenceEqual(str);
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
            """, """
                 using System;

                 public class TestClass {
                     public void TestMethod() {
                         var span = "TEST".AsSpan(0, 2);
                         char[] str = ['T', 'E'];
                         var compare = span.SequenceEqual(str);
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
            """, """
                 using System;

                 public class TestClass {
                     public void TestMethod() {
                         var span = "TEST".ToCharArray().AsSpan(0, 2);
                         char[] str = ['T', 'E'];
                         var compare = span.SequenceEqual(str);
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span.SequenceEqual(GetStr());
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".ToCharArray().AsSpan(0, 2);
                             var compare = span.SequenceEqual(GetStr());
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".AsSpan(0, 2);
                             var compare = span.SequenceEqual(Str);
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
                """, """
                     using System;

                     public class TestClass {
                         public void TestMethod() {
                             var span = "TEST".ToCharArray().AsSpan(0, 2);
                             var compare = span.SequenceEqual(Str);
                         }

                         public char[] Str => ['T', 'E'];
                     }
                     """)]
    public Task Test(string source, string fixedSource) => TestStatic(source, fixedSource);
}
