﻿namespace Bluehill.Analyzers.Tests;

public sealed class BH0009AnalyzerTest : BHAnalyzerTest<BH0009DontDivideByConstantZeroAnalyzer> {
    [Theory]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(sbyte i) {
                        _ = [|i / (sbyte)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(sbyte i) {
                        _ = [|i % (sbyte)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(byte i) {
                        _ = [|i / (byte)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(byte i) {
                        _ = [|i % (byte)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(short i) {
                        _ = [|i / (short)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(short i) {
                        _ = [|i % (short)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(ushort i) {
                        _ = [|i / (ushort)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(ushort i) {
                        _ = [|i % (ushort)0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(int i) {
                        _ = [|i / 0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(int i) {
                        _ = [|i % 0|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(uint i) {
                        _ = [|i / 0u|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(uint i) {
                        _ = [|i % 0u|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(long i) {
                        _ = [|i / 0l|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(long i) {
                        _ = [|i % 0l|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(ulong i) {
                        _ = [|i / 0ul|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(ulong i) {
                        _ = [|i % 0ul|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(decimal d) {
                        _ = [|d / 0.0m|];
                    }
                }
                """)]
    [InlineData("""
                public class TestClass {
                    public void TestMethod(decimal d) {
                        _ = [|d % 0.0m|];
                    }
                }
                """)]
    public Task Test(string source) => TestStatic(source);
}
