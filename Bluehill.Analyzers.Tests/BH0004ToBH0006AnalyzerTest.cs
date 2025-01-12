using Verify =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Bluehill.Analyzers.BH0004ToBH0006Analyzer,
        Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Bluehill.Analyzers.Tests;

public sealed class BH0004ToBH0006AnalyzerTest {
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
        """
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
        """
        )]
    [InlineData(
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
        """
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
        """
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
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() {
                return null;
            }
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
        }
        """
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
        """
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
        """
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
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public void Test() {
                {|BH0006:GetSchema()|};
            }
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public void Test() {
                {|BH0006:((IXmlSerializable)this).GetSchema()|};
            }
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public void Test() => {|BH0006:GetSchema()|};
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public void Test() => {|BH0006:((IXmlSerializable)this).GetSchema()|};
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public XmlSchema? Test() {
                return {|BH0006:GetSchema()|};
            }
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public XmlSchema? Test() {
                return {|BH0006:((IXmlSerializable)this).GetSchema()|};
            }
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            public XmlSchema? {|BH0004:GetSchema|}() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public XmlSchema? Test() => {|BH0006:GetSchema()|};
        }
        """
        )]
    [InlineData(
        """
        using System;
        using System.Xml;
        using System.Xml.Schema;
        using System.Xml.Serialization;

        public class Class1 : IXmlSerializable {
            XmlSchema? IXmlSerializable.GetSchema() => null;
            public void ReadXml(XmlReader reader) => throw new NotImplementedException();
            public void WriteXml(XmlWriter writer) => throw new NotImplementedException();
            public XmlSchema? Test() => {|BH0006:((IXmlSerializable)this).GetSchema()|};
        }
        """
        )]
    public Task Test(string source) => Verify.VerifyAnalyzerAsync(source);
}
