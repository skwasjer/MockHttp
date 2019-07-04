using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MockHttp.Extensions
{
	public class HttpResponseMessageExtensionsTests
	{
		[Fact]
		public async Task Given_null_response_when_cloning_should_return_null_response()
		{
			HttpResponseMessage response = null;

			// Act
			// ReSharper disable once ExpressionIsAlwaysNull
			HttpResponseMessage clone = await response.CloneResponseAsync();

			// Assert
			clone.Should().BeNull();
		}

		[Fact]
		public async Task Given_response_when_cloning_should_return_cloned_response()
		{
			var response = new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.Created,
				ReasonPhrase = "A created response",
				Headers =
				{
					{ "Response", "Header" }
				},
				Content = new StringContent("data"),
				Version = new Version(3, 2),
				RequestMessage = new HttpRequestMessage()
			};

			// Act
			HttpResponseMessage clone = await response.CloneResponseAsync();

			// Assert
			clone.Should().BeEquivalentTo(response);
			// Content is not asserted by equivalency (the content headers are!), since there is no property that returns the content data.
			(await clone.Content.ReadAsByteArrayAsync()).Should().BeEquivalentTo(await response.Content.ReadAsByteArrayAsync());
		}
	}
}
