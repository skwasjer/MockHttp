using System;
using System.Collections.Generic;
using System.Linq;

namespace MockHttp.Http
{
	internal class HttpHeaderEqualityComparer : IEqualityComparer<KeyValuePair<string, IEnumerable<string>>>
	{
		public bool Equals(KeyValuePair<string, IEnumerable<string>> x, KeyValuePair<string, IEnumerable<string>> y)
		{
			if (x.Key != y.Key)
			{
				return false;
			}

			if (y.Value.Any(
				yValue => x.Value
					.SelectMany(HttpHeadersCollection.ParseHttpHeaderValue)
					.All(xValue => HttpHeadersCollection.ParseHttpHeaderValue(yValue).Contains(xValue))
				))
			{
				return true;
			}

			return !x.Value.Any();
		}

		public int GetHashCode(KeyValuePair<string, IEnumerable<string>> obj)
		{
			throw new NotImplementedException();
		}
	}
}