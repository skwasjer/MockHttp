using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class QueryStringMatcherTests
	{
		private QueryStringMatcher _sut;

		[Theory]
		[InlineData("?key=value", "key", "value")]
		[InlineData("?key1=value1&key2=value2", "key2", "value2")]
		[InlineData("?key=value1&key=value2", "key", "value1")]
		[InlineData("?key1=value1&%C3%A9%C3%B4x%C3%84=%24%25%5E%26*&key2=value", "éôxÄ", "$%^&*")]
		public void Given_queryString_equals_expected_queryString_when_matching_should_match(string queryString, string expectedKey, string expectedValue)
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri("http://localhost/" + queryString)
			};

			_sut = new QueryStringMatcher(new[]
			{
				new KeyValuePair<string, IEnumerable<string>>(expectedKey, new [] { expectedValue })
			});

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Theory]
		[InlineData("")]
		[InlineData("?")]
		[InlineData("?key=value")]
		[InlineData("?key1=value1&key2=value2")]
		[InlineData("?key=value1&key=value2")]
		public void Given_queryString_does_not_equal_expected_queryString_when_matching_should_not_match(string queryString)
		{
			var request = new HttpRequestMessage
			{
				RequestUri = new Uri("http://localhost/" + queryString)
			};

			_sut = new QueryStringMatcher(new[]
			{
				new KeyValuePair<string, IEnumerable<string>>("key_not_in_uri", null)
			});

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}

	}
}
