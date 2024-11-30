namespace Dolly;
public static class ObjectExtensionMethods
{
    public static IEnumerable<T> RecursiveFlatten<T>(this T value, Func<T, T?> getChild) where T : class
    {
        yield return value;
        var child = getChild(value);
        if (child != null)
        {
            foreach (var tmp in child.RecursiveFlatten(getChild))
            {
                yield return tmp;

            }
        }
    }

    public static IEnumerable<T> RecursiveFlatten<T>(this T value, Func<T, IEnumerable<T>> getChildren) where T : class
    {
        yield return value;
        var children = getChildren(value);
        foreach (var child in children)
        {
            foreach (var tmp in child.RecursiveFlatten(getChildren))
            {
                yield return tmp;
            }
        }
    }
}
