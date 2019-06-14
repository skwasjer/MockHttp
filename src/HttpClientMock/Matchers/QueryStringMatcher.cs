using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using HttpClientMock.Http;

namespace HttpClientMock.Matchers
{
	public class QueryStringMatcher : IHttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly QueryString _matchQs;

		public QueryStringMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			_matchQs = new QueryString(values);
		}

		public QueryStringMatcher(string queryString)
			: this(QueryString.Parse(queryString))
		{
		}

		public bool IsMatch(HttpRequestMessage request)
		{
			QueryString query = QueryString.Parse(request.RequestUri.Query);
			return _matchQs.All(q => query.ContainsKey(q.Key) && query[q.Key].Any(qv => q.Value.Contains(qv)));
		}

		public override string ToString()
		{
			return $"Query string: '{_matchQs}'";
		}
	}
}
