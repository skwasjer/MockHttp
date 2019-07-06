using System;
using System.Collections.Generic;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Http
{
	public class QueryStringTests
	{
		[Fact]
		public void Given_null_queryString_when_parsing_should_throw()
		{
			// Act
			Action act = () => QueryString.Parse(null);

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.WithParamName("queryString");
		}

		[Fact]
		public void Given_empty_queryString_when_parsing_should_return_empty_dictionary()
		{
			// Act
			QueryString actual = QueryString.Parse(string.Empty);

			// Assert
			actual.Should().BeEmpty();
		}

		[Fact]
		public void Given_queryString_starts_with_questionMark_when_parsing_should_ignore_it()
		{
			var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value" } } };

			// Act
			QueryString actual = QueryString.Parse("?key=value");

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void Given_queryString_contains_hashTerminator_when_parsing_should_ignore_it()
		{
			var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value" } } };

			// Act
			QueryString actual = QueryString.Parse("key=value#hash");

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void Given_queryString_contains_encoded_key_or_value_when_parsing_should_url_decode()
		{
			var expected = new Dictionary<string, IEnumerable<string>> { { "éôxÄ", new[] { "$%^&*" } } };

			// Act
			QueryString actual = QueryString.Parse("%C3%A9%C3%B4x%C3%84=%24%25%5E%26*");

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void Given_queryString_contains_multiple_same_keys_when_parsing_should_combine_into_single_entry()
		{
			var expected = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^ &*", "another value" } } };

			// Act
			QueryString actual = QueryString.Parse("?key=value&key=%24%25%5E%20%26%2A&key=another%20value");

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void Given_queryString_contains_multiple_same_keys_when_formatting_should_produce_correct_string()
		{
			var queryStringData = new Dictionary<string, IEnumerable<string>> { { "key", new[] { "value", "$%^&*", "another value" } } };
			const string expected = "?key=value&key=%24%25%5E%26%2A&key=another%20value";
			var queryString = new QueryString(queryStringData);

			// Act
			string actual = queryString.ToString();

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}

		[Fact]
		public void Given_queryString_contains_variation_of_keys_with_or_without_value_when_parsing_should_return_keys_with_correct_values()
		{
			var expected = new Dictionary<string, IEnumerable<string>>
			{
				{ "key1", new string[0] },
				{ "key2", new [] { "value" } },
				{ "key3", new [] { "" } },
				{ "key4", new [] { "value1", "value2" } },
			};

			// Act
			QueryString actual = QueryString.Parse("?key1&key2=value&key3=&key4=value1&key4=value2");

			// Assert
			actual.Should().BeEquivalentTo(expected);
		}
	}
}
