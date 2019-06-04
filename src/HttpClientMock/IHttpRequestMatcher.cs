using System.Net.Http;

namespace HttpClientMock
{
	/// <summary>
	/// Describes an expectation for matching a <see cref="HttpRequestMessage"/>.
	/// </summary>
	public interface IHttpRequestMatcher
	{
		/// <summary>
		/// Checks that the request matches the expectation.
		/// </summary>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		bool IsMatch(HttpRequestMessage request);
	}
}