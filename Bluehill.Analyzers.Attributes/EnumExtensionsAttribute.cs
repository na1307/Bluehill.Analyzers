namespace Bluehill;

[Conditional("BLUEHILL_ANALYZERS_ATTRIBUTES")]
[AttributeUsage(AttributeTargets.Enum)]
public sealed class EnumExtensionsAttribute : Attribute;
