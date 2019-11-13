using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MockHttp.Responses;
using Xunit;

namespace MockHttp.Json
{
	public class JsonContentMatcherTests
	{
		[Theory]
		[MemberData(nameof(JsonMatchTestCases))]
		public async Task Given_source_json_when_matching_it_should_succeed(object expectedJsonContentAsObject, string requestJson)
		{
			var request = new HttpRequestMessage
			{
                Content = new StringContent(requestJson)
			};
			var context = new MockHttpRequestContext(request);
			var sut = new JsonContentMatcher(expectedJsonContentAsObject);

			// Act
			bool result = await sut.IsMatchAsync(context);

            // Assert
            result.Should().BeTrue();
		}

		public static IEnumerable<object[]> JsonMatchTestCases()
		{
			yield return new object[] { new [] { 1, 2, 3 }, "[1,2,3]" };
			yield return new object[] { new { value = 1 }, "{\"value\":1}" };
			yield return new object[] { "some text", "\"some text\"" };
			yield return new object[] { 123, "123" };
		}
	}
}
