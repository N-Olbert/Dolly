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
    /// <summary>
    /// Can only occur when <see cref="MemberFlags.Enumerable"/> is present
    /// Can never occur at the same time as <see cref="MemberFlags.NewCompatible"/>
    /// </summary>
    ArrayCompatible = 4,
    /// <summary>
    /// Can only occur when <see cref="MemberFlags.Enumerable"/> is present
    /// Can never occur at the same time as <see cref="MemberFlags.ArrayCompatible"/>
    /// </summary>
    NewCompatible = 8,
    MemberValueType = 16,
    MemberNullable = 32,
    ElementValueType = 64,
    ElementNullable = 128,
}

[DebuggerDisplay("{Name}")]
public sealed record Member(string Name, bool IsReadonly, MemberFlags Flags)
{
    public bool IsEnumerable => Flags.HasFlag(MemberFlags.Enumerable);
    public bool IsClonable => Flags.HasFlag(MemberFlags.Clonable);
    public bool IsArrayCompatible => Flags.HasFlag(MemberFlags.ArrayCompatible);
    public bool IsNewCompatible => Flags.HasFlag(MemberFlags.NewCompatible);
    public bool IsMemberValueType => Flags.HasFlag(MemberFlags.MemberValueType);
    public bool IsMemberNullable => Flags.HasFlag(MemberFlags.MemberNullable);
    public bool IsElementValueType => Flags.HasFlag(MemberFlags.ElementValueType);
    public bool IsElementNullable => Flags.HasFlag(MemberFlags.ElementNullable);

    public static Member Create(string name, bool isReadonly, bool nullabilityEnabled, ITypeSymbol symbol)
    {
        var flags = GetFlags(symbol, nullabilityEnabled);            
        return new Member(name, isReadonly, flags);

    }

    private static MemberFlags GetFlags(ITypeSymbol symbol, bool nullabilityEnabled)
    {
        MemberFlags flags = MemberFlags.None;
        if (symbol.IsNullable(nullabilityEnabled))
        {
            flags |= MemberFlags.MemberNullable;
        }
        if (symbol.IsValueType)
        {
            flags |= MemberFlags.MemberValueType;
        }

        // Handle Array
        if (symbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            flags |= MemberFlags.Enumerable;
            flags |= MemberFlags.ArrayCompatible;
            if (arrayTypeSymbol.IsClonable())
            {
                flags |= MemberFlags.Clonable;
            }
            if (arrayTypeSymbol.IsNullable(nullabilityEnabled))
            {
                flags |= MemberFlags.ElementNullable;
            }
            if (arrayTypeSymbol.IsValueType)
            {
                flags |= MemberFlags.ElementValueType;
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
            if (namedSymbol.TypeArguments[0].IsNullable(nullabilityEnabled))
            {
                flags |= MemberFlags.ElementNullable;
            }
            if (namedSymbol.TypeArguments[0].IsValueType)
            {
                flags |= MemberFlags.ElementValueType;
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
            if (enumerableType.IsNullable(nullabilityEnabled))
            {
                flags |= MemberFlags.ElementNullable;
            }
            if (enumerableType.IsValueType)
            {
                flags |= MemberFlags.ElementValueType;
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
            if (IsMemberNullable)
            {
                builder.Append(Name);
                builder.Append(" == null ? null : new (");
            }
            else
            {
                builder.Append("new (");
            }
        }
        builder.Append(Name);
        if (IsMemberNullable && !IsNewCompatible)
        {
            builder.Append("?");
        }
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