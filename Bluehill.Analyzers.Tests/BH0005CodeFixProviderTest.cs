using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Bluehill.Analyzers.BH0004ToBH0006Analyzer,
    Bluehill.Analyzers.BH0005CodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0005CodeFixProviderTest {
    private const string FixedCodeNotExplicit =
        """
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

    private const string FixedCodeExplicit =
        """
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

    private const string FixedCodeAbstract =
        """
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
        FixedCodeNotExplicit)]
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
        FixedCodeNotExplicit)]
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
        FixedCodeNotExplicit)]
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
        FixedCodeNotExplicit)]
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
        FixedCodeExplicit)]
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
        FixedCodeExplicit)]
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
        FixedCodeAbstract)]
    public Task Test(string source, string fixedSource) => Verify.VerifyCodeFixAsync(source, fixedSource);
}
