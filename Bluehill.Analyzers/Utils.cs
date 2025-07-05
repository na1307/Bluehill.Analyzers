namespace Bluehill.Analyzers;

internal static class Utils {
    public static readonly SymbolDisplayFormat FqnFormat
        = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

    public static string GetTypeName(string fqnName, string? @namespace)
        => !string.IsNullOrWhiteSpace(@namespace) ? fqnName.Substring($"{@namespace}.".Length) : fqnName;

    public static string GetEscapedName(string name) => name.Replace('.', '_');

    public static Accessibility GetTypeAccessibility(ISymbol symbol) {
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

    public static Accessibility GetMethodAccessibility(ISymbol symbol) {
        var ab = symbol.DeclaredAccessibility;

        return ab is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal ? ab : Accessibility.NotApplicable;
    }
}
