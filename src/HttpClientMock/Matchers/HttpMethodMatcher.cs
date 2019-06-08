using System;
using System.Net.Http;

namespace HttpClientMock.Matchers
{
	public class HttpMethodMatcher : IHttpRequestMatcher
	{
		public HttpMethodMatcher(string method)
			: this(new HttpMethod(method))
		{
		}

		public HttpMethodMatcher(HttpMethod method)
		{
			ExpectedMethod = method ?? throw new ArgumentNullException(nameof(method));
		}

		public HttpMethod ExpectedMethod { get; }

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			return request.Method == ExpectedMethod;
		}

		public override string ToString()
		{
			return $"Method: {ExpectedMethod.Method}";
		}
	}
}
