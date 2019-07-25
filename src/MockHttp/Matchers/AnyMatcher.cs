using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by verifying it against a list of constraints, for which at least one has to match the request.
	/// </summary>
	public class AnyMatcher : HttpRequestMatcher
	{
		private readonly IReadOnlyCollection<HttpRequestMatcher> _matchers;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnyMatcher"/> class using specified list of <paramref name="matchers"/>.
		/// </summary>
		/// <param name="matchers">A list of matchers for which at least one has to match.</param>
		public AnyMatcher(IReadOnlyCollection<HttpRequestMatcher> matchers)
		{
			_matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			return _matchers.Any(request);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			if (_matchers.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.AppendLine("Any:");
			sb.AppendLine("{");
			foreach (HttpRequestMatcher m in _matchers)
			{
				sb.Append('\t');
				sb.AppendLine(m.ToString());
			}

			sb.Append("}");
			return sb.ToString();
		}
	}
}