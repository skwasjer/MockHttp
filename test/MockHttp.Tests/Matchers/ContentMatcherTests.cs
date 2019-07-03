using System.Net.Http;
using System.Text;
using FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class ContentMatcherTests
	{
		private ContentMatcher _sut;

		[Theory]
		[InlineData("request content", "utf-8")]
		[InlineData("", "utf-8")]
		[InlineData("pasākumi", "utf-16")]
		public void Given_request_content_equals_expected_content_when_matching_should_match(string content, string encoding)
		{
			string expectedContent = content;
			Encoding enc = Encoding.GetEncoding(encoding);

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content, enc)
			};

			_sut = new ContentMatcher(expectedContent, enc);

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

			_sut = new ContentMatcher(expectedContent);

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

			_sut = new ContentMatcher(expectedContent);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_request_content_is_empty_and_expected_content_is_also_empty_when_matching_should_match()
		{
			var request = new HttpRequestMessage();

			_sut = new ContentMatcher();

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_request_content_is_empty_and_expected_content_is_not_when_matching_should_not_match()
		{
			var request = new HttpRequestMessage();

			_sut = new ContentMatcher("some data");

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}
	}
}
