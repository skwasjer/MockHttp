using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace HttpClientMock.Matchers
{
	public class AnyMatcher : IHttpRequestMatcher
	{
		private readonly IReadOnlyCollection<IHttpRequestMatcher> _matchers;

		public AnyMatcher(IReadOnlyCollection<IHttpRequestMatcher> matchers)
		{
			_matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
		}

		public bool IsMatch(HttpRequestMessage request)
		{
			return _matchers.Any(request);
		}

		public override string ToString()
		{
			if (_matchers.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.AppendLine("Any:");
			sb.AppendLine("{");
			foreach (IHttpRequestMatcher m in _matchers)
			{
				sb.Append('\t');
				sb.AppendLine(m.ToString());
			}

			sb.Append("}");
			return sb.ToString();
		}
	}
}