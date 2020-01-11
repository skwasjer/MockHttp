using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request URI query string.
	/// </summary>
	public class QueryStringMatcher : HttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly QueryString _matchQs;

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringMatcher"/> class using specified query string parameters.
		/// </summary>
		/// <param name="parameters">The query string parameters.</param>
		public QueryStringMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> parameters)
		{
			if (parameters is null)
			{
				throw new ArgumentNullException(nameof(parameters));
			}

			_matchQs = new QueryString(parameters);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringMatcher"/> class using specified query string.
		/// </summary>
		/// <param name="queryString">The query string.</param>
		public QueryStringMatcher(string queryString)
			: this(QueryString.Parse(queryString))
		{
		}

		/// <inheritdoc />
		public override bool IsMatch(MockHttpRequestContext requestContext)
		{
			QueryString query = QueryString.Parse(requestContext.Request.RequestUri.Query);

			// When match collection is empty, behavior is flipped, and we expect no query string parameters on request.
			if (_matchQs.Count == 0 && query.Count > 0)
			{
				return false;
			}

			return _matchQs.All(q => query.ContainsKey(q.Key)
			 && (BothAreEmpty(query[q.Key], q.Value)
			 || HasOneOf(query[q.Key], q.Value))
			);
		}

		private static bool BothAreEmpty(IEnumerable<string> left, IEnumerable<string> right)
		{
			int leftCount = left.Count();
			int rightCount = right.Count();
			return leftCount == 0 && leftCount == rightCount;
		}

		private static bool HasOneOf(IEnumerable<string> left, IEnumerable<string> right)
		{
			return left.Any(right.Contains);
		}

		/// <inheritdoc />
		public override bool IsExclusive => _matchQs.Count == 0;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"QueryString: '{_matchQs}'";
		}
	}
}
