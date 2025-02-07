namespace Bluehill.Analyzers;

internal static class Utils {
    public static readonly SymbolDisplayFormat FqnFormat
        = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public static string GetTypeName(string fqnName, string? @namespace)
        => @namespace is not null ? fqnName.Substring($"{@namespace}.".Length) : fqnName;

    public static string GetEscapedTypeName(string typeName) => typeName.Replace('.', '_');

    public static Accessibility GetAccessibility(ISymbol symbol) {
        while (true) {
            var ab = symbol.DeclaredAccessibility;

            if (ab is not (Accessibility.Public or Accessibility.Internal)) {
                return Accessibility.NotApplicable;
            }

            if (ab is Accessibility.Internal) {
                return Accessibility.Internal;
            }

            var cs = symbol.ContainingSymbol;

            if (cs is INamespaceSymbol) {
                return Accessibility.Public;
            }

            symbol = cs;
        }
    }
}
