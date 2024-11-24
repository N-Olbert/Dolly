using Microsoft.CodeAnalysis;

namespace Dolly;

public static class ISymbolExtensionMethods
{
    public static bool HasAttribute(this ISymbol symbol, string name, string @namespace = "Dolly") =>
        symbol.GetAttributes().Any(a => a.AttributeClass != null && a.AttributeClass.ContainingNamespace.ToCodeString() == @namespace && a.AttributeClass.Name == name);

    public static bool TryGetIEnumerableType(this ISymbol symbol, out ITypeSymbol enumerableType)
    {
        if (symbol.TryGetGenericIEnumerable(out var @interface))
        {
            enumerableType = @interface.TypeArguments[0];
            return true;
        }
        enumerableType = null!;
        return false;
    }

    public static bool TryGetGenericIEnumerable(this ISymbol symbol, out INamedTypeSymbol @interface)
    {
        if (symbol is INamedTypeSymbol selfNamedSymbol && selfNamedSymbol.IsGenericIEnumerable())
        {
            @interface = selfNamedSymbol;
            return true;
        }

        @interface = null!;
        if (symbol is INamedTypeSymbol namedSymbol)
        {
            @interface = namedSymbol.AllInterfaces.SingleOrDefault(@interface => @interface.IsGenericIEnumerable())!;
            return @interface != null;
        }
        return false;
    }

    public static bool IsClonable(this ITypeSymbol typeSymbol) =>
        typeSymbol.HasAttribute("ClonableAttribute") || typeSymbol.AllInterfaces.Any(i => i.Name == "IClone");

    public static string GetFullTypeName(this ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedSymbol)
        {
            return $"{symbol.ContainingNamespace.ToCodeString()}.{symbol.Name}{(namedSymbol.IsGenericType ? $"<{string.Join(", ", namedSymbol.TypeArguments.Select(ta => ta.GetFullTypeName()))}>" : "")}";

        }
        return $"{symbol.ContainingNamespace.ToCodeString()}.{symbol.Name}";
    }

    public static bool IsGenericIEnumerable(this ISymbol symbol) =>
        symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericType && namedSymbol.ConstructedFrom.IsGenericIEnumerableDefinition();

    public static bool IsGenericIEnumerableDefinition(this ISymbol symbol) => 
        symbol is INamedTypeSymbol namedSymbol && 
        namedSymbol.ContainingNamespace.ToCodeString() == "System.Collections.Generic" && 
        namedSymbol.Name == "IEnumerable" &&
        namedSymbol.TypeArguments.Length == 1;
}
