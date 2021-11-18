using System.Net.Http.Headers;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Http
{
	public class HttpHeadersCollectionTests
	{
		[Fact]
		public void Given_null_header_when_parsing_should_throw()
		{
			// Act
			Action act = () => HttpHeadersCollection.Parse(null);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.WithParamName("headers");
		}

		[Fact]
		public void Given_empty_header_when_parsing_should_return_empty_dictionary()
		{
			// Act
			HttpHeaders actual = HttpHeadersCollection.Parse(string.Empty);

			// Assert
			actual.Should().BeEmpty();
		}

		[Theory]
		[InlineData("key:single value", "key", "single value")]
		[InlineData("key:value1,value2", "key", "value1", "value2")]
		[InlineData("allow-spaces-in-field-value: value1 , value2 ", "allow-spaces-in-field-value", "value1", "value2")]
		public void Given_single_header_string_when_parsing_should_return_key_with_one_or_more_values(string headerString, string expectedHeaderKey, params string[] expectedValues)
		{
			// Act
			HttpHeaders headers = HttpHeadersCollection.Parse(headerString);

			// Assert
			headers.TryGetValues(expectedHeaderKey, out IEnumerable<string> values)
				.Should()
				.BeTrue();
			values
				.Should()
				.BeEquivalentTo(expectedValues);
		}

		[Theory]
		[InlineData("no-separator", "The value cannot be null or empty.*")]
		[InlineData(":value", "The value cannot be null or empty.*")]
		[InlineData(" :value", "The header name format is invalid.")]
		[InlineData(" leading-whitespace-not-allowed:value", "The header name format is invalid.")]
		[InlineData("\tleading-whitespace-not-allowed:value", "The header name format is invalid.")]
		[InlineData("trailing-whitespace-not-allowed :value", "The header name format is invalid.")]
		[InlineData("trailing-whitespace-not-allowed\t:value", "The header name format is invalid.")]
		public void Given_single_invalid_header_string_with_when_parsing_should_throw(string invalidHeaderString, string exceptionMessage)
		{
			// Act
			Action act = () => HttpHeadersCollection.Parse(invalidHeaderString);

			// Assert
			act.Should().Throw<Exception>()
				.WithMessage(exceptionMessage);
		}

		[Fact]
		public void Given_multiple_empty_newlines_when_parsing_should_ignore_newlines()
		{
			string headers = Environment.NewLine + Environment.NewLine + "Header: Value";

			// Act
			HttpHeaders actual = HttpHeadersCollection.Parse(headers);

			// Assert
			actual.Should().HaveCount(1);
			actual.Should().Contain(h => h.Key == "Header");
		}
	}
}
