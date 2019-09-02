using System.Net.Http;
using System.Threading.Tasks;
using MockHttp.Responses;

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
		/// <param name="requestContext">The request context.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		Task<bool> IsMatchAsync(MockHttpRequestContext requestContext);

		/// <summary>
		/// Gets whether the matcher is mutually exclusive to other matchers of the same type.
		/// </summary>
		bool IsExclusive { get; }
	}
}