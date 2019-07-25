using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Moq;
using Xunit;

namespace MockHttp.Matchers
{
	public class AnyMatcherTests
	{
		private readonly List<HttpRequestMatcher> _matchers;
		private readonly AnyMatcher _sut;

		public AnyMatcherTests()
		{
			_matchers = new List<HttpRequestMatcher>();
			_sut = new AnyMatcher(_matchers);
		}

		[Theory]
		[InlineData("url1")]
		[InlineData("url2")]
		public void Given_request_uri_equals_one_of_the_matchers_when_matching_should_match(string requestUrl)
		{
			_matchers.Add(new RequestUriMatcher("*url1"));
			_matchers.Add(new RequestUriMatcher("*url2"));

			var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue("the request url '{0}' should match one of the matchers", requestUrl);
		}

		[Fact]
		public void Given_request_uri_matches_none_of_the_matchers_when_matching_should_not_match()
		{
			_matchers.Add(new RequestUriMatcher("http://127.0.0.1/"));
			_matchers.Add(new RequestUriMatcher("http://127.0.0.2/"));

			var request = new HttpRequestMessage(HttpMethod.Get, "http://127.0.0.3/");

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

		[Fact]
		public void When_formatting_should_return_human_readable_representation()
		{
			const string innerMatcherText = "Type: text";
			string expectedText = string.Format(@"Any:{1}{{{1}	{0}1{1}	{0}2{1}}}", innerMatcherText, Environment.NewLine);
			var matcherMock1 = new Mock<HttpRequestMatcher>();
			var matcherMock2 = new Mock<HttpRequestMatcher>();
			matcherMock1.Setup(m => m.ToString()).Returns(innerMatcherText + "1");
			matcherMock2.Setup(m => m.ToString()).Returns(innerMatcherText + "2");
			_matchers.Add(matcherMock1.Object);
			_matchers.Add(matcherMock2.Object);

			// Act
			string displayText = _sut.ToString();

			// Assert
			displayText.Should().Be(expectedText);
		}
	}
}
