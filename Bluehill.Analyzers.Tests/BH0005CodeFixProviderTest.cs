using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<Bluehill.Analyzers.BH0004AndBH0005Analyzer,
        Bluehill.Analyzers.BH0005CodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0005CodeFixProviderTest {
    private const string fixedCodeNotExplicit = """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """;
    private const string fixedCodeExplicit = """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """;
    private const string fixedCodeAbstract = """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public abstract class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """;

    [Theory]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() {|BH0005:=> throw new NotImplementedException()|};
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeNotExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() {|BH0005:=> new()|};
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeNotExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() {|BH0005:{
                throw new NotImplementedException();
            }|}
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeNotExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() {|BH0005:{
                return new();
            }|}
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeNotExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() {|BH0005:=> throw new NotImplementedException()|};
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() {|BH0005:=> new()|};
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeExplicit
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public abstract class Class1 : IXmlSerializable {
            {|BH0005:public abstract XmlSchema? {|BH0004:GetSchema|}();|}
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """,
        fixedCodeAbstract
        )]
    public Task Test(string source, string fixedSource) => Verify.VerifyCodeFixAsync(source, fixedSource);
}
