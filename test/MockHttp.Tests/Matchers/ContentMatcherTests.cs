using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Responses;
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
		public async Task Given_request_content_equals_expected_content_when_matching_should_match(string content, string encoding)
		{
			string expectedContent = content;
			Encoding enc = Encoding.GetEncoding(encoding);

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content, enc)
			};

			_sut = new ContentMatcher(expectedContent, enc);

			// Act & assert
			(await _sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
		}

		[Fact]
		public async Task Given_request_content_does_not_equal_expected_content_when_matching_should_not_match()
		{
			const string content = "some http request content";
			const string expectedContent = "expected content";

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content)
			};

			_sut = new ContentMatcher(expectedContent, Encoding.UTF8);

			// Act & assert
			(await _sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
		}

		[Fact]
		public async Task Given_request_content_equals_expected_content_when_matching_twice_should_match_twice()
		{
			const string content = "some http request content";
			const string expectedContent = content;

			var request = new HttpRequestMessage
			{
				Content = new StringContent(content)
			};

			_sut = new ContentMatcher(expectedContent, Encoding.UTF8);

			// Act & assert
			var ctx = new MockHttpRequestContext(request);
			(await _sut.IsMatchAsync(ctx)).Should().BeTrue();
			(await _sut.IsMatchAsync(ctx)).Should().BeTrue("the content should be buffered and matchable more than once");
		}

		[Fact]
		public async Task Given_request_content_is_empty_and_expected_content_is_also_empty_when_matching_should_match()
		{
			var request = new HttpRequestMessage();

			_sut = new ContentMatcher();

			// Act & assert
			(await _sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
		}

		[Fact]
		public async Task Given_request_content_is_empty_and_expected_content_is_not_when_matching_should_not_match()
		{
			var request = new HttpRequestMessage();

			_sut = new ContentMatcher("some data", Encoding.UTF8);

			// Act & assert
			(await _sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
		}

		[Fact]
		public void Given_null_content_string_with_encoding_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new ContentMatcher(null, Encoding.UTF8);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("content");
		}

		[Fact]
		public void Given_content_string_with_null_encoding_when_creating_matcher_should_not_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new ContentMatcher("data", null);

			// Assert
			act.Should().NotThrow("default encoding should be used instead");
		}

		[Fact]
		public void Given_null_content_bytes_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new ContentMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("content");
		}

		[Theory]
		[InlineData("text response data", "Content: text response data")]
		[InlineData("", "Content: <empty>")]
		[InlineData("ByteArray", "Content: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79]")]
		[InlineData("ByteArrays", "Content: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79,0x73]")]
		[InlineData("ByteArrayWithBigDataGetsTruncated", "Content: [0x42,0x79,0x74,0x65,0x41,0x72,0x72,0x61,0x79,0x57,...](Size = 33)")]
		public void When_formatting_should_return_human_readable_representation(string content, string expectedText)
		{
			// If theory starts with ByteArray string, we will actually act as if the content was in binary form (thus no encoding)
			_sut = content.StartsWith("ByteArray")
				? new ContentMatcher(Encoding.UTF8.GetBytes(content))
				: new ContentMatcher(content, Encoding.UTF8);

			// Act
			string displayText = _sut.ToString();

			// Assert
			displayText.Should().Be(expectedText);
		}

		[Fact]
		public async Task Given_null_context_when_matching_it_should_throw()
		{
			_sut = new ContentMatcher("", null);
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
