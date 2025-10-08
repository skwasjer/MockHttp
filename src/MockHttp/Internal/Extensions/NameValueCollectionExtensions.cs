using System.Collections.Specialized;

namespace MockHttp.Extensions;

internal static class NameValueCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, IEnumerable<string?>>> AsEnumerable(this NameValueCollection nameValueCollection)
    {
        if (nameValueCollection is null)
        {
            throw new ArgumentNullException(nameof(nameValueCollection));
        }

        return Iterator(nameValueCollection);

        static IEnumerable<KeyValuePair<string, IEnumerable<string?>>> Iterator(NameValueCollection nvc)
        {
            foreach (string key in nvc)
            {
                yield return new KeyValuePair<string, IEnumerable<string?>>(
                    key,
                    nvc.GetValues(key) ?? []
                );
            }
        }
    }
}
