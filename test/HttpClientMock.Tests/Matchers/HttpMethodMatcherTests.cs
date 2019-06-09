using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace HttpClientMock.Matchers
{
	public class HttpMethodMatcherTests
	{
		private HttpMethodMatcher _sut;

		[Theory]
		[InlineData("GET")]
		[InlineData("POST")]
		[InlineData("ARBITRARY")]
		public void Given_request_method_equals_expected_method_when_matching_should_match(string httpMethod)
		{
			var request = new HttpRequestMessage
			{
				Method = new HttpMethod(httpMethod)
			};

			_sut = new HttpMethodMatcher(httpMethod);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Theory]
		[InlineData("GET")]
		[InlineData("POST")]
		[InlineData("ARBITRARY")]
		public void Given_request_method_does_not_equal_expected_method_when_matching_should_not_match(string httpMethod)
		{
			var request = new HttpRequestMessage
			{
				Method = new HttpMethod(httpMethod)
			};

			_sut = new HttpMethodMatcher("PUT");

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}
	}
}
