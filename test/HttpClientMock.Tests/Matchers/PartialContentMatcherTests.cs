using System.Net.Http;
using System.Text;
using FluentAssertions;
using Xunit;

namespace HttpClientMock.Matchers
{
	public class PartialContentMatcherTests
	{
		private PartialContentMatcher _sut;

		[Theory]
		[InlineData("request content", "utf-8")]
		[InlineData("pasākumi", "utf-16")]
		public void Given_request_content_equals_expected_content_when_matching_should_match(string content, string encoding)
		{
			string expectedContent = content;
			Encoding enc = Encoding.GetEncoding(encoding);

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content, enc)
			};

			_sut = new PartialContentMatcher(expectedContent, enc);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_request_content_does_not_equal_expected_content_when_matching_should_not_match()
		{
			const string content = "some http request content";
			const string expectedContent = "expected content";

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content)
			};

			_sut = new PartialContentMatcher(expectedContent);

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}

		[Fact]
		public void Given_request_content_equals_expected_content_when_matching_twice_should_match_twice()
		{
			const string content = "some http request content";
			const string expectedContent = content;

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content)
			};

			_sut = new PartialContentMatcher(expectedContent);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_request_content_is_empty_and_expected_content_is_not_when_matching_should_not_match()
		{
			var request = new HttpRequestMessage();

			_sut = new PartialContentMatcher("some data");

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}

		[Theory]
		[InlineData("match at beginning", "match", "utf-8")]
		[InlineData("match at end", "end", "utf-8")]
		[InlineData("match in middle", "in m", "utf-16")]
		public void Given_request_content_contains_expected_content_when_matching_should_match(string content, string expectedContent, string encoding)
		{
			Encoding enc = Encoding.GetEncoding(encoding);

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content, enc)
			};

			_sut = new PartialContentMatcher(expectedContent, enc);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}
	}
}
