using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MockHttp.Matchers;

namespace MockHttp
{
	internal static class HttpRequestMatcherExtensions
	{
		/// <summary>
		/// Checks if all <paramref name="matchers"/> match the given <paramref name="request"/>.
		/// </summary>
		/// <param name="matchers">The matchers to check against the <paramref name="request"/>.</param>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true" /> if all <paramref name="matchers"/> match the <paramref name="request"/>.</returns>
		public static async Task<bool> AllAsync(this IEnumerable<IAsyncHttpRequestMatcher> matchers, HttpRequestMessage request)
		{
			var noMatchList = new List<IAsyncHttpRequestMatcher>();
			bool hasMatchedAll = true;
			foreach (IAsyncHttpRequestMatcher m in matchers)
			{
				if (await m.IsMatchAsync(request).ConfigureAwait(false))
				{
					continue;
				}

				noMatchList.Add(m);
				hasMatchedAll = false;
			}

//			notMatchedOn = noMatchList;
			return hasMatchedAll;
		}

		/// <summary>
		/// Checks if any <paramref name="matchers"/> match the given <paramref name="request"/>.
		/// </summary>
		/// <param name="matchers">The matchers to check against the <paramref name="request"/>.</param>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true" /> if any <paramref name="matchers"/> match the <paramref name="request"/>.</returns>
		public static async Task<bool> AnyAsync(this IEnumerable<IAsyncHttpRequestMatcher> matchers, HttpRequestMessage request)
		{
			foreach (IAsyncHttpRequestMatcher m in matchers)
			{
				if (await m.IsMatchAsync(request).ConfigureAwait(false))
				{
					return true;
				}
			}

			return false;
		}
	}
}
