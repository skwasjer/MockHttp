using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class AnyMatcherTests
	{
		private readonly List<IHttpRequestMatcher> _matchers;
		private readonly AnyMatcher _sut;

		public AnyMatcherTests()
		{
			_matchers = new List<IHttpRequestMatcher>();
			_sut = new AnyMatcher(_matchers);
		}

		[Theory]
		[InlineData("url1")]
		[InlineData("url2")]
		public void Given_request_uri_equals_one_of_the_matchers_when_matching_should_match(string requestUrl)
		{
			_matchers.Add(new UrlMatcher("url1"));
			_matchers.Add(new UrlMatcher("url2"));

			var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue("the request url '{0}' should match one of the matchers", requestUrl);
		}

		[Fact]
		public void Given_request_uri_matches_none_of_the_matchers_when_matching_should_not_match()
		{
			_matchers.Add(new UrlMatcher("http://127.0.0.1"));
			_matchers.Add(new UrlMatcher("http://127.0.0.2"));

			var request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.3");

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse("the request url should not match any of the matchers");
		}

		[Fact]
		public void Given_null_matchers_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new AnyMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("matchers");
		}
	}
}
