using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
#if !NETCOREAPP1_1
using System.Net.Http.Formatting;
#endif
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MockHttp.Extensions
{
	public class HttpContentExtensionsTests
	{
		[Fact]
		public async Task Given_null_content_when_cloning_should_return_null_content()
		{
			HttpContent content = null;

			// Act
			// ReSharper disable once ExpressionIsAlwaysNull
			ByteArrayContent clone = await content.CloneAsByteArrayContentAsync();

			// Assert
			clone.Should().BeNull();
		}

		[Theory]
		[MemberData(nameof(ContentTestCases))]
		public async Task Given_content_when_cloning_should_return_byte_array_content_with_same_data_and_headers(HttpContent content, string expectedData)
		{
			// Act
			ByteArrayContent clone = await content.CloneAsByteArrayContentAsync();

			// Assert
			clone.Should().NotBeNull();
			clone.Headers.Should().BeEquivalentTo(content.Headers);

			string actual = await clone.ReadAsStringAsync();
			actual.Should().Be(expectedData);
		}

		[Theory]
		[MemberData(nameof(ContentTestCases))]
		public async Task Given_content_when_cloning_multiple_times_should_not_throw(HttpContent content, string expectedData)
		{
			// Act
			for (int i = 0; i < 3; i++)
			{
				ByteArrayContent clone = null;
				Func<Task> act = async () => clone = await content.CloneAsByteArrayContentAsync();

				// Assert
				act.Should().NotThrow();
				clone.Should().NotBeNull();
				clone.Headers.Should().BeEquivalentTo(content.Headers, "iteration {0} should have same headers", i);

				string actual = await clone.ReadAsStringAsync();
				actual.Should().Be(expectedData, "iteration {0} should have same data", i);
			}
		}

		public static IEnumerable<object[]> ContentTestCases()
		{
			object[] CreateTestCase(HttpContent content, string expectedData)
			{
				content.Headers.Add("Custom", "Header");
				content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
				return new object[] { content, expectedData };
			}

			const string data = "<b>data</b>";
			byte[] buffer = Encoding.UTF8.GetBytes(data);
			yield return CreateTestCase(new StringContent(data), data);
			yield return CreateTestCase(new ByteArrayContent(buffer), data);
			yield return CreateTestCase(new StreamContent(new MemoryStream(buffer)), data);
			yield return CreateTestCase(new FormUrlEncodedContent(new Dictionary<string, string> { { "key", data } }), "key=" + Uri.EscapeDataString(data));

			var mpc = new MultipartContent("subtype", "boundary");
			mpc.Add(new StringContent(data));
			yield return CreateTestCase(mpc, $"--boundary{Environment.NewLine}Content-Type: text/plain; charset=utf-8{Environment.NewLine}{Environment.NewLine}<b>data</b>{Environment.NewLine}--boundary--{Environment.NewLine}");
#if !NETCOREAPP1_1
			yield return CreateTestCase(new ObjectContent(typeof(string), data, new JsonMediaTypeFormatter()), $"\"{data}\"");
#endif
#if NETCOREAPP2_1 || NETCOREAPP3_1
			yield return CreateTestCase(new ReadOnlyMemoryContent(buffer.AsMemory()), data);
#endif
		}
	}
}
