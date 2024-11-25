using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;

namespace Dolly;

[Flags]
public enum MemberFlags
{
    None = 0,
    Clonable = 1,
    Enumerable = 2,
    ArrayCompatible = 4,
    NewCompatible = 8
}

[DebuggerDisplay("{Name}")]
sealed record Member(string Name, bool IsReadonly, MemberFlags Flags)
{
    public bool IsEnumerable => (Flags & MemberFlags.Enumerable) == MemberFlags.Enumerable;
    public bool IsClonable => (Flags & MemberFlags.Clonable) == MemberFlags.Clonable;
    public bool IsArrayCompatible => (Flags & MemberFlags.ArrayCompatible) == MemberFlags.ArrayCompatible;
    public bool IsNewCompatible => (Flags & MemberFlags.NewCompatible) == MemberFlags.NewCompatible;

    public static Member Create(string name, bool isReadonly, ITypeSymbol symbol)
    {
        var flags = GetFlags(symbol);            
        return new Member(name, isReadonly, flags);

    }

    private static MemberFlags GetFlags(ITypeSymbol symbol)
    {
        MemberFlags flags = MemberFlags.None;

        // Handle Array
        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            flags |= MemberFlags.Enumerable;
            flags |= MemberFlags.ArrayCompatible;
            if (arrayTypeSymbol.IsClonable())
            {
                flags |= MemberFlags.Clonable;
            }
        }
        // Handle Enumerable
        else if (symbol is INamedTypeSymbol namedSymbol && namedSymbol.IsGenericIEnumerable())
        {
            flags |= MemberFlags.Enumerable;
            flags |= MemberFlags.ArrayCompatible;
            if (namedSymbol.TypeArguments[0].IsClonable())
            {
                flags |= MemberFlags.Clonable;
            }
        }
        // Handle types that implement IEnumerable<T> and take IEnumerable<T> as constructor parameter (ConcurrentQueue<T>, List<T>, ConcurrentStack<T> and LinkedList<T>)
        else if (symbol is INamedTypeSymbol namedSymbol2 &&
            namedSymbol2.TryGetIEnumerableType(out var enumerableType) &&
            namedSymbol2.Constructors.Any(c =>
            c.Parameters.Length == 1 &&
            c.Parameters[0].Type.TryGetIEnumerableType(out var constructorEnumerableType) &&
            SymbolEqualityComparer.Default.Equals(enumerableType, constructorEnumerableType)))
        {
            flags |= MemberFlags.Enumerable;
            flags |= MemberFlags.NewCompatible;
            if (enumerableType.IsClonable())
            {
                flags |= MemberFlags.Clonable;
            }
        }
        else if (symbol.IsClonable())
        {
            flags = MemberFlags.Clonable;
        }
        return flags;
    }

    public string ToString(bool deepClone)
    {
        var builder = new StringBuilder();
        if (IsNewCompatible)
        {
            builder.Append("new (");
        }
        builder.Append(Name);
        if (IsEnumerable)
        {
            if (IsClonable && deepClone)
            {
                builder.Append(".Select(item => item.Clone())");
            }
            if (IsArrayCompatible)
            {
                builder.Append(".ToArray()");
            }
        }
        else
        {
            if (IsClonable && deepClone)
            {
                builder.Append(".Clone()");
            }
        }
        if (IsNewCompatible)
        {
            builder.Append(")");
        }
        return builder.ToString();
    }
}