using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class HttpHeadersMatcherTests
	{
		private HttpHeadersMatcher _sut;

		[Fact]
		public void Given_request_contains_expected_headers_when_matching_should_match()
		{
			var request = new HttpRequestMessage
			{
				Headers =
				{
					CacheControl = CacheControlHeaderValue.Parse("public, max-age=31536000, must-revalidate"),
					Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json"), MediaTypeWithQualityHeaderValue.Parse("text/html;q=0.9") }
				}
			};

			_sut = new HttpHeadersMatcher(new Dictionary<string, IEnumerable<string>>
			{
				{ "Cache-Control", new [] { "must-revalidate", "public", "max-age=31536000" } },
				{ "Accept", new [] { "application/json" } }
			});

			// Act & assert
			_sut.IsMatch(request).Should().BeTrue();
		}
	}
}
