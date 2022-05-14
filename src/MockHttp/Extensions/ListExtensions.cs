namespace MockHttp;

internal static class ListExtensions
{
    internal static void Replace<T>(this IList<T> list, T instance)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        bool IsExact(Type thisType)
        {
            return thisType == instance.GetType();
        }

        int[] existing = list
            .Select((item, index) => new { item, index })
            .Where(x =>
            {
                Type thisType = x.item?.GetType();
                return IsExact(thisType);
            })
            .Select(x => x.index)
            .ToArray();
        for (int i = existing.Length - 1; i >= 0; i--)
        {
            list.RemoveAt(existing[i]);
        }

        list.Add(instance);
    }
}
