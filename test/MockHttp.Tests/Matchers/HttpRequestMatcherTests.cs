using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Responses;
using Moq;
using Xunit;

namespace MockHttp.Matchers
{
	public class HttpRequestMatcherTests
	{
		private readonly HttpRequestMatcher _sut;

		public HttpRequestMatcherTests()
		{
			_sut = new Mock<HttpRequestMatcher> { CallBase = true }.Object;
		}

		[Fact]
		public async Task Given_null_context_when_matching_it_should_throw()
		{
			MockHttpRequestContext requestContext = null;

			// Act
			// ReSharper disable once ExpressionIsAlwaysNull
			Func<Task> act = () => _sut.IsMatchAsync(requestContext);

			// Assert
			await act.Should()
				.ThrowAsync<ArgumentNullException>()
				.WithParamName(nameof(requestContext));
		}
	}
}
