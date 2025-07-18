#pragma warning disable
using System.Diagnostics.CodeAnalysis;

namespace Bluehill;

[Conditional("BLUEHILL_ANALYZERS_ATTRIBUTES")]
[Experimental("BHDECORATOR0001",
    Message = "This attribute is very unstable and its behavior is very likely to change in the future. Do not use it in production.",
    UrlFormat = "https://bluehillnuget.github.io/Bluehill.Analyzers/{0}")]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DecoratorAttribute(string decoratorFqn) : Attribute;
