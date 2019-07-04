using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class HttpHeadersMatcherTests
	{
		private HttpHeadersMatcher _sut;

		[Fact]
		public void Given_request_contains_expected_headers_when_matching_should_match()
		{
			DateTimeOffset lastModified = DateTimeOffset.UtcNow;
			HttpRequestMessage request = GetRequestWithHeaders(lastModified);

			_sut = new HttpHeadersMatcher(new Dictionary<string, IEnumerable<string>>
			{
				{ "Cache-Control", new [] { "must-revalidate", "public", "max-age=31536000" } },
				{ "Accept", new [] { "application/json" } },
				{ "Last-Modified", new [] { lastModified.ToString("R", CultureInfo.InvariantCulture) } },
				{ "Content-Length", new [] { "123" } }
			});

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_single_value_should_match()
		{
			HttpRequestMessage request = GetRequestWithHeaders();

			_sut = new HttpHeadersMatcher("Cache-Control", "must-revalidate");

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Theory]
		[InlineData("must-revalidate", "public", "max-age=31536000")]
		[InlineData("public", "must-revalidate")]
		public void Given_request_contains_expected_header_with_multiple_values_when_matching_for_all_values_should_match_irregardless_of_order(params string[] values)
		{
			HttpRequestMessage request = GetRequestWithHeaders();

			_sut = new HttpHeadersMatcher("Cache-Control", values);

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}

		[Fact]
		public void Given_null_header_name_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new HttpHeadersMatcher(null, (string)null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("name");
		}

		[Fact]
		public void Given_null_header_name_for_multiple_values_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new HttpHeadersMatcher(null, (string[])null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("name");
		}

		[Fact]
		public void Given_null_headers_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new HttpHeadersMatcher((IEnumerable<KeyValuePair<string, string>>)null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("headers");
		}

		[Fact]
		public void Given_null_headers_for_multiple_values_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new HttpHeadersMatcher((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("headers");
		}

		private static HttpRequestMessage GetRequestWithHeaders(DateTimeOffset? lastModified = null)
		{
			var request = new HttpRequestMessage
			{
				Headers =
				{
					CacheControl = CacheControlHeaderValue.Parse("public, max-age=31536000, must-revalidate"),
					Accept =
					{
						MediaTypeWithQualityHeaderValue.Parse("application/json"),
						MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.9")
					}
				},
				Content = new StringContent("")
				{
					Headers =
					{
						LastModified = lastModified,
						ContentLength = 123
					}
				}
			};
			return request;
		}
	}
}
