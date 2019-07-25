using System.Net.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Represents a condition for matching a <see cref="HttpRequestMessage"/>.
	/// </summary>
	public abstract class HttpRequestMatcher
	{
		/// <summary>
		/// Checks that the request matches a condition.
		/// </summary>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		public abstract bool IsMatch(HttpRequestMessage request);

		/// <summary>
		/// Gets whether the matcher is mutually exclusive to other matchers of the same type.
		/// </summary>
		public virtual bool IsExclusive { get; } = false;

		/// <inheritdoc />
		public abstract override string ToString();
	}
}