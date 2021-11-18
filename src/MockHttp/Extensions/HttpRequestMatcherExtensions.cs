using System.Collections.Generic;
using System.Threading.Tasks;
using MockHttp.Matchers;
using MockHttp.Responses;

namespace MockHttp
{
	internal static class HttpRequestMatcherExtensions
	{
		/// <summary>
		/// Checks if all <paramref name="matchers"/> match the given <paramref name="requestContext"/>.
		/// </summary>
		/// <param name="matchers">The matchers to check against the <paramref name="requestContext"/>.</param>
		/// <param name="requestContext">The request context.</param>
		/// <returns><see langword="true" /> if all <paramref name="matchers"/> match the <paramref name="requestContext"/>.</returns>
		public static async Task<bool> AllAsync(this IEnumerable<IAsyncHttpRequestMatcher> matchers, MockHttpRequestContext requestContext)
		{
			bool hasMatchedAll = true;
			foreach (IAsyncHttpRequestMatcher m in matchers)
			{
				if (await m.IsMatchAsync(requestContext).ConfigureAwait(false))
				{
					continue;
				}

				hasMatchedAll = false;
			}

			return hasMatchedAll;
		}

		/// <summary>
		/// Checks if any <paramref name="matchers"/> match the given <paramref name="requestContext"/>.
		/// </summary>
		/// <param name="matchers">The matchers to check against the <paramref name="requestContext"/>.</param>
		/// <param name="requestContext">The request context.</param>
		/// <returns><see langword="true" /> if any <paramref name="matchers"/> match the <paramref name="requestContext"/>.</returns>
		public static async Task<bool> AnyAsync(this IEnumerable<IAsyncHttpRequestMatcher> matchers, MockHttpRequestContext requestContext)
		{
			foreach (IAsyncHttpRequestMatcher m in matchers)
			{
				if (await m.IsMatchAsync(requestContext).ConfigureAwait(false))
				{
					return true;
				}
			}

			return false;
		}
	}
}
