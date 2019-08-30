using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Represents a condition for matching a <see cref="HttpRequestMessage"/>.
	/// </summary>
	public interface IAsyncHttpRequestMatcher
	{
		/// <summary>
		/// Checks that the request matches a condition.
		/// </summary>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		Task<bool> IsMatchAsync(HttpRequestMessage request);

		/// <summary>
		/// Gets whether the matcher is mutually exclusive to other matchers of the same type.
		/// </summary>
		bool IsExclusive { get; }
	}
}