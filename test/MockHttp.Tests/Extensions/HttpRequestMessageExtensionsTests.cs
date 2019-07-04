using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MockHttp.Extensions
{
	public class HttpRequestMessageExtensionsTests
	{
		[Fact]
		public async Task Given_null_request_when_cloning_should_return_null_request()
		{
			HttpRequestMessage request = null;

			// Act
			// ReSharper disable once ExpressionIsAlwaysNull
			HttpRequestMessage clone = await request.CloneRequestAsync();

			// Assert
			clone.Should().BeNull();
		}

		[Fact]
		public async Task Given_request_when_cloning_should_return_cloned_request()
		{
			var request = new HttpRequestMessage
			{
				Method = new HttpMethod("VERB"),
				RequestUri = new Uri("http://0.0.0.0/url?query"),
				Headers =
				{
					{ "Request", "Header" }
				},
				Properties =
				{
					{ "Property1", 1 },
					{ "Property2", true },
					{ "Property3", new object() }
				},
				Content = new StringContent("data"),
				Version = new Version(3, 2)
			};

			// Act
			HttpRequestMessage clone = await request.CloneRequestAsync();

			// Assert
			clone.Should().BeEquivalentTo(request);
			// Content is not asserted by equivalency (the content headers are!), since there is no property that returns the content data.
			(await clone.Content.ReadAsByteArrayAsync()).Should().BeEquivalentTo(await request.Content.ReadAsByteArrayAsync());
		}
	}
}
