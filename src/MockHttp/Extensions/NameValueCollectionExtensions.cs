#if !NETSTANDARD1_1
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MockHttp
{
	internal static class NameValueCollectionExtensions
	{
		public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> AsEnumerable(this NameValueCollection nameValueCollection)
		{
			if (nameValueCollection == null)
			{
				throw new ArgumentNullException(nameof(nameValueCollection));
			}

			foreach (string key in nameValueCollection)
			{
				yield return new KeyValuePair<string, IEnumerable<string>>(
					key,
					nameValueCollection.GetValues(key)
				);
			}
		}
	}
}
#endif