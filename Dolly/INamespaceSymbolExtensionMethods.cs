using Microsoft.CodeAnalysis;

namespace Dolly;

public static class INamespaceSymbolExtensionMethods
{
    public static string ToCodeString(this INamespaceSymbol symbol) => 
        symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
}
