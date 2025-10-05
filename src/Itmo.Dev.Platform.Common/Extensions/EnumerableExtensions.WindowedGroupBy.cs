using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

public static partial class EnumerableExtensions
{
    /// <summary>
    ///     Groups the elements of the sequence into windows according to selected key. "Windows" means that the
    ///     grouping will contain elements with same CONSECUTIVE key. That means that produced sequence of groupings
    ///     may contain multiple groupings with the same key.
    /// </summary>
    /// <example>
    ///
    ///     If we define an array of key-value pair and then group them: <br/>
    /// 
    ///     <code>
    ///         var collection = new[]
    ///         {
    ///             (1, 1),
    ///             (1, 2),
    ///             (2, 3),
    ///             (1, 4)
    ///         };
    ///         
    ///         var groupings = collection.WindowedGroupBy(tuple => tuple.Item1).ToArray();
    ///     </code>
    ///     <br/>
    /// 
    ///     We will get the result like this: <br/><br/>
    ///
    ///     groupings[0] = 1 -> [1, 2] <br/>
    ///     groupings[1] = 2 -> [3] <br/>
    ///     groupings[2] = 1 -> [4] <br/>
    ///     
    /// </example>
    /// <returns></returns>
    public static IEnumerable<IGrouping<TKey, TElement>> WindowedGroupBy<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer)
    {
        comparer ??= EqualityComparer<TKey>.Default;

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext() is false)
            yield break;

        var previousKey = keySelector.Invoke(enumerator.Current);

        var currentGrouping = new WindowedGrouping<TKey, TElement>(previousKey)
        {
            elementSelector.Invoke(enumerator.Current),
        };

        while (enumerator.MoveNext())
        {
            var key = keySelector.Invoke(enumerator.Current);

            if (comparer.Equals(previousKey, key) is false)
            {
                yield return currentGrouping;

                currentGrouping = new WindowedGrouping<TKey, TElement>(key)
                {
                    elementSelector.Invoke(enumerator.Current),
                };

                previousKey = key;
            }
            else
            {
                currentGrouping.Add(elementSelector.Invoke(enumerator.Current));
            }
        }

        yield return currentGrouping;
    }

    /// <inheritdoc cref="WindowedGroupBy{TSource,TKey,TElement}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TKey},System.Func{TSource,TElement},System.Collections.Generic.IEqualityComparer{TKey}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IGrouping<TKey, TElement>> WindowedGroupBy<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector)
    {
        return WindowedGroupBy(
            source,
            keySelector,
            elementSelector,
            comparer: null);
    }

    /// <inheritdoc cref="WindowedGroupBy{TSource,TKey,TElement}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TKey},System.Func{TSource,TElement},System.Collections.Generic.IEqualityComparer{TKey}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IGrouping<TKey, TSource>> WindowedGroupBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer)
    {
        return WindowedGroupBy(
            source,
            keySelector,
            elementSelector: static element => element,
            comparer);
    }

    /// <inheritdoc cref="WindowedGroupBy{TSource,TKey,TElement}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TKey},System.Func{TSource,TElement},System.Collections.Generic.IEqualityComparer{TKey}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IGrouping<TKey, TSource>> WindowedGroupBy<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        return WindowedGroupBy(
            source,
            keySelector,
            comparer: null);
    }

    private class WindowedGrouping<TKey, TElement>(TKey key)
        : IGrouping<TKey, TElement>
    {
        private readonly List<TElement> _elements = [];

        public TKey Key => key;

        public IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TElement element) => _elements.Add(element);
    }
}
