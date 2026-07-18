using System.Collections;

namespace Itmo.Dev.Platform.Persistence.Postgres.Models;

internal sealed class OneWayProxyList<TSource, TTarget>(
    IReadOnlyList<TSource> source,
    Func<TSource, TTarget> selector)
    : IList<TTarget>
{
    public int Count => source.Count;
    public bool IsReadOnly => true;

    public TTarget this[int index]
    {
        get => selector(source[index]);
        set => throw new NotSupportedException();
    }

    public void CopyTo(TTarget[] array, int arrayIndex)
    {
        foreach (TSource element in source)
        {
            array[arrayIndex++] = selector(element);
        }
    }

    public IEnumerator<TTarget> GetEnumerator()
    {
        foreach (TSource element in source)
            yield return selector(element);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TTarget item) => throw new NotSupportedException();

    public void Clear() => throw new NotSupportedException();

    public bool Contains(TTarget item) => throw new NotSupportedException();

    public bool Remove(TTarget item) => throw new NotSupportedException();

    public int IndexOf(TTarget item) => throw new NotSupportedException();

    public void Insert(int index, TTarget item) => throw new NotSupportedException();

    public void RemoveAt(int index) => throw new NotSupportedException();
}
