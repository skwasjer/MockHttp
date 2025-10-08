using System.Collections;
using System.Diagnostics;

namespace MockHttp.Threading;

internal class ConcurrentCollection<T> : IReadOnlyList<T>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly object _syncLock = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<T>? _items;

    public void Add(T item)
    {
        lock (_syncLock)
        {
            _items ??= [];
            _items.Add(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        // Take local copies of collection and count so they are isolated from changes by other threads.
        List<T> items;
        int count;

        lock (_syncLock)
        {
            if (_items is null)
            {
                yield break;
            }

            items = _items;
            count = _items.Count;
        }

        for (int i = 0; i < count; i++)
        {
            yield return items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count
    {
        get
        {
            lock (_syncLock)
            {
                return _items?.Count ?? 0;
            }
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _items = null;
        }
    }

    public T this[int index]
    {
        get
        {
            lock (_syncLock)
            {
                if (_items is null)
                {
#pragma warning disable S112
#pragma warning disable CA2201 // Do not raise reserved exception types
                    throw new IndexOutOfRangeException();
#pragma warning restore CA2201 // Do not raise reserved exception types
#pragma warning restore S112
                }

                return _items[index];
            }
        }
    }
}
