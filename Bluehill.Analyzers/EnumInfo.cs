using System.Collections.ObjectModel;

namespace Bluehill.Analyzers;

internal readonly record struct EnumInfo(string FqnName, string? Namespace, Accessibility Accessibility, bool IsFlags, ReadOnlyCollection<string> Members);
