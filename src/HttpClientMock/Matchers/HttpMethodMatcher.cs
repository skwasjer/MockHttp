using System;
using System.Net.Http;

namespace HttpClientMock.Matchers
{
	public class HttpMethodMatcher : ValueMatcher<HttpMethod>
	{
		public HttpMethodMatcher(string method)
			: this(new HttpMethod(method))
		{
		}

		public HttpMethodMatcher(HttpMethod method)
			: base(method)
		{
			if (method == null)
			{
				throw new ArgumentNullException(nameof(method));
			}
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			return request.Method == Value;
		}

		public override string ToString()
		{
			return $"Method: {Value.Method}";
		}
	}
}
