using System.Collections;
using System.Collections.Immutable;

namespace Dolly;

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>, IReadOnlyList<T>
{
    private readonly ImmutableArray<T> array = ImmutableArray<T>.Empty;

    public EquatableArray(T[] array)
    {
        this.array = ImmutableArray.Create(array);
    }

    public T this[int index] => array[index];

    public int Count => array.Length;

    public bool Equals(EquatableArray<T> other) => array.SequenceEqual(other.array);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)array).GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)array).GetEnumerator();

    public override int GetHashCode()
    {

        if (array.Length == 0)
        {
            return 0;
        }
        HashCode hashCode = default;

        foreach (T item in array)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }

    public static implicit operator EquatableArray<T>(T[] source) =>
        new EquatableArray<T>(source);

    public static EquatableArray<T> Empty() => new EquatableArray<T>([]);
}