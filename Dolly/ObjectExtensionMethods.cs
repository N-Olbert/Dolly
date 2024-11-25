using System;
using System.Collections.Generic;
using System.Text;

namespace Dolly;
internal static class ObjectExtensionMethods
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
}
