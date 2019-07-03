using System.Net.Http;

namespace HttpClientMock.Matchers
{
	public abstract class ValueMatcher<T> : IHttpRequestMatcher
	{
		protected ValueMatcher(T value)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the expected value.
		/// </summary>
		protected T Value { get; }

		/// <inheritdoc />
		public abstract bool IsMatch(HttpRequestMessage request);
	}
}
