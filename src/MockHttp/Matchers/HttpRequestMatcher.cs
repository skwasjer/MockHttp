using System;
using System.Net.Http;
using System.Threading.Tasks;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Represents a condition for matching a <see cref="HttpRequestMessage"/>.
	/// </summary>
	public abstract class HttpRequestMatcher : IAsyncHttpRequestMatcher
	{
		/// <summary>
		/// Checks that the request matches a condition.
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		public Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
		{
			if (requestContext is null)
			{
				throw new ArgumentNullException(nameof(requestContext));
			}

			return Task.FromResult(IsMatch(requestContext));
		}

		/// <summary>
		/// Checks that the request matches a condition.
		/// </summary>
		/// <param name="requestContext">The request context.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		public abstract bool IsMatch(MockHttpRequestContext requestContext);

		/// <summary>
		/// Gets whether the matcher is mutually exclusive to other matchers of the same type.
		/// </summary>
		public virtual bool IsExclusive { get; } = false;

		/// <inheritdoc />
		public abstract override string ToString();
	}
}
