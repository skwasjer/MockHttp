﻿using System;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
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

			_sut = new HttpMethodMatcher(new HttpMethod(httpMethod));

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

			_sut = new HttpMethodMatcher(HttpMethod.Put);

			// Act & assert
			_sut.IsMatch(request).Should().BeFalse();
		}

		[Fact]
		public void Given_null_method_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new HttpMethodMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("method");
		}

		[Fact]
		public void When_formatting_should_return_human_readable_representation()
		{
			const string expectedText = "Method: OPTIONS";
			_sut = new HttpMethodMatcher(HttpMethod.Options);

			// Act
			string displayText = _sut.ToString();

			// Assert
			displayText.Should().Be(expectedText);
		}
	}
}
