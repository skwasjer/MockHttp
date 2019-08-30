using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by verifying it against a list of constraints, for which at least one has to match the request.
	/// </summary>
	public class AnyMatcher : IAsyncHttpRequestMatcher
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnyMatcher"/> class using specified list of <paramref name="matchers"/>.
		/// </summary>
		/// <param name="matchers">A list of matchers for which at least one has to match.</param>
		public AnyMatcher(IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers)
		{
			Matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
		}

		/// <summary>
		/// Gets the inner matchers.
		/// </summary>
		public IReadOnlyCollection<IAsyncHttpRequestMatcher> Matchers { get; }

		/// <inheritdoc />
		public Task<bool> IsMatchAsync(HttpRequestMessage request)
		{
			return Matchers.AnyAsync(request);
		}

		/// <inheritdoc />
		public bool IsExclusive { get; } = false;

		/// <inheritdoc />
		public override string ToString()
		{
			if (Matchers.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.AppendLine("Any:");
			sb.AppendLine("{");
			foreach (IAsyncHttpRequestMatcher m in Matchers)
			{
				sb.Append('\t');
				sb.AppendLine(m.ToString());
			}

			sb.Append("}");
			return sb.ToString();
		}
	}
}