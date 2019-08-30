using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Matchers;
using Xunit;

namespace MockHttp.Extensions
{
	public abstract class RequestMatchingExtensionsTests
	{
		private readonly RequestMatching _sut;

		protected RequestMatchingExtensionsTests()
		{
			_sut = new RequestMatching();
		}

		public class RequestUri : RequestMatchingExtensionsTests
		{
			[Fact]
			public async Task When_configuring_requestUri_as_string_should_match()
			{
				// Act
				_sut.RequestUri("http://127.0.0.1/");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<RequestUriMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1/")
				})).Should().BeTrue();
			}

			[Fact]
			public async Task When_configuring_requestUri_as_uri_should_match()
			{
				var uri = new Uri("http://127.0.0.1/");

				// Act
				_sut.RequestUri(uri);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<RequestUriMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1/")
				})).Should().BeTrue();
			}
		}

		public class QueryString : RequestMatchingExtensionsTests
		{
			[Fact]
			public async Task When_configuring_query_string_with_null_value_should_match()
			{
				// Act
				_sut.QueryString("key", (string)null);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key")
				})).Should().BeTrue();
			}

			[Fact]
			public async Task When_configuring_query_string_with_empty_value_should_match()
			{
				// Act
				_sut.QueryString("key", string.Empty);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key=")
				})).Should().BeTrue();
			}

			[Theory]
			[InlineData("?key1=value1", true)]
			[InlineData("key2=value2&key1=value1", true)]
			[InlineData("http://127.0.0.1?key1=value1&key2=value2", true)]
			[InlineData("?other=value1", false)]
			public async Task When_configuring_query_string_should_match(string queryString, bool expectedResult)
			{
				// Act
				_sut.QueryString(queryString);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key1=value1&key2=value2")
				})).Should().Be(expectedResult);
			}

			[Fact]
			public void When_configuring_query_string_with_null_key_should_throw()
			{
				// Act
				Action act = () => _sut.QueryString(null, string.Empty);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("key");
			}

			[Fact]
			public void When_configuring_query_string_with_null_values_should_not_throw()
			{
				// Act
				Action act = () => _sut.QueryString("key", (IEnumerable<string>)null);

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public void When_configuring_null_list_of_query_string_parameters_should_throw()
			{
				// Act
				Action act = () => _sut.QueryString((IEnumerable<KeyValuePair<string, IEnumerable<string>>>)null);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("parameters");
			}

			[Fact]
			public void When_configuring_empty_query_string_should_throw()
			{
				// Act
				Action act = () => _sut.QueryString(string.Empty);

				// Assert
				act.Should().Throw<ArgumentException>().WithParamName("queryString").WithMessage("Specify a query string*");
			}

			[Fact]
			public void When_configuring_null_query_string_should_throw()
			{
				// Act
				Action act = () => _sut.QueryString((string)null);

				// Assert
				act.Should().Throw<ArgumentException>().WithParamName("queryString").WithMessage("Specify a query string*");
			}


			[Theory]
			[InlineData("?")]
			[InlineData("")]
			public async Task When_configuring_without_query_string_should_match(string queryString)
			{
				// Act
				_sut.WithoutQueryString();
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1" + queryString)
				})).Should().BeTrue();
			}

			[Fact]
			public void Given_query_string_matcher_is_already_added_when_configuring_another_should_not_throw()
			{
				_sut.QueryString("key", "value");

				// Act
				Action act = () => _sut.QueryString("key1", "value1");

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public void Given_query_string_matcher_is_already_added_when_configuring_without_querystring_should_throw()
			{
				_sut.QueryString("key", "value");

				// Act
				Action act = () => _sut.WithoutQueryString();

				// Assert
				act.Should().Throw<InvalidOperationException>();
			}
		}

		public class Method : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("GET", false)]
			[InlineData("POST", true)]
			public async Task When_configuring_httpMethod_should_match(string httpMethod, bool expectedResult)
			{
				// Act
				_sut.Method(httpMethod);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpMethodMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Method = new HttpMethod("POST")
				})).Should().Be(expectedResult);
			}

			[Fact]
			public void Given_httpMethod_matcher_is_already_added_when_configuring_another_should_throw()
			{
				_sut.Method("GET");

				// Act
				Action act = () => _sut.Method("GET");

				// Assert
				act.Should().Throw<InvalidOperationException>();
			}
		}

		public class ContentType : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("text/plain; charset=us-ascii", true)]
			[InlineData("text/html; charset=us-ascii", false)]
			[InlineData("text/plain; charset=utf-8", false)]
			public async Task When_configuring_contentType_should_match(string mediaType, bool expectedResult)
			{
				// Act
				_sut.ContentType(mediaType);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<MediaTypeHeaderMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent(string.Empty, Encoding.ASCII, "text/plain")
				})).Should().Be(expectedResult);
			}

			[Fact]
			public void Given_mediaType_matcher_is_already_added_when_configuring_another_should_throw()
			{
				_sut.ContentType("text/plain");

				// Act
				Action act = () => _sut.ContentType("text/html");

				// Assert
				act.Should().Throw<InvalidOperationException>();
			}

			[Fact]
			public void When_configuring_media_type_with_null_header_value_should_throw()
			{
				// Act
				Action act = () => _sut.ContentType((MediaTypeHeaderValue)null);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("mediaType");
			}

			[Fact]
			public void When_configuring_media_type_with_null_string_should_throw()
			{
				// Act
				Action act = () => _sut.ContentType((string)null);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("mediaType");
			}

			[Fact]
			public void When_configuring_media_type_with_null_string_and_encoding_should_throw()
			{
				// Act
				Action act = () => _sut.ContentType(null, Encoding.UTF8);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("contentType");
			}

			[Fact]
			public void When_configuring_media_type_with_string_and_null_encoding_should_not_throw()
			{
				// Act
				Action act = () => _sut.ContentType("text/plain", null);

				// Assert
				act.Should().NotThrow<ArgumentNullException>();
			}
		}

		public class Content : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("content", true)]
			[InlineData("more content", false)]
			public async Task When_configuring_content_should_match(string content, bool expectedResult)
			{
				// Act
				_sut.Content(content);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("content")
				})).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("utf-8", true)]
			[InlineData("us-ascii", false)]
			public async Task When_configuring_content_with_encoding_should_match(string encoding, bool expectedResult)
			{
				// Act
				_sut.Content("straße", Encoding.GetEncoding(encoding));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("straße", Encoding.UTF8)
				})).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("content", true)]
			[InlineData("more content", false)]
			public async Task When_configuring_content_with_stream_should_match(string content, bool expectedResult)
			{
				byte[] data = Encoding.UTF8.GetBytes(content);
				using (var ms = new MemoryStream(data))
				{
					// Act
					_sut.Content(ms);
					IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

					// Assert
					matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
					(await matchers.AnyAsync(new HttpRequestMessage
					{
						Content = new StringContent("content")
					})).Should().Be(expectedResult);
				}
			}

			[Fact]
			public void Given_content_matcher_is_already_added_when_configuring_should_throw()
			{
				_sut.Content("content");

				// Act
				Action act = () => _sut.Content("content");

				// Assert
				act.Should().Throw<InvalidOperationException>();
			}
		}

		public class PartialContent : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("one two three", true)]
			[InlineData("four", false)]
			[InlineData("two", true)]
			public async Task When_configuring_partial_content_should_match(string content, bool expectedResult)
			{
				// Act
				_sut.PartialContent(content);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("one two three")
				})).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("utf-8", true)]
			[InlineData("us-ascii", false)]
			public async Task When_configuring_partial_content_with_encoding_should_match(string encoding, bool expectedResult)
			{
				// Act
				_sut.PartialContent("ß", Encoding.GetEncoding(encoding));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("straße", Encoding.UTF8)
				})).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("one two three", true)]
			[InlineData("four", false)]
			[InlineData("two", true)]
			public async Task When_configuring_partial_content_with_stream_should_match(string content, bool expectedResult)
			{
				byte[] data = Encoding.UTF8.GetBytes(content);
				using (var ms = new MemoryStream(data))
				{
					// Act
					_sut.PartialContent(ms);
					IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

					// Assert
					matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
					(await matchers.AnyAsync(new HttpRequestMessage
					{
						Content = new StringContent("one two three")
					})).Should().Be(expectedResult);
				}
			}

			[Fact]
			public void Given_partial_content_matcher_is_already_added_when_configuring_should_throw()
			{
				_sut.PartialContent("content");

				// Act
				Action act = () => _sut.PartialContent("content");

				// Assert
				act.Should().NotThrow();
			}

			[Fact]
			public async Task Given_two_partial_content_matchers_when_request_matcher_should_be_true()
			{
				// Act
				_sut.PartialContent("one");
				_sut.PartialContent("three");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("one two three", Encoding.UTF8)
				})).Should().BeTrue();
			}
		}

		public class Headers : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("2019-07-26T10:34:06.012345Z", true)]
			[InlineData("2019-07-26T10:34:06.012Z", true)]
			[InlineData("2019-07-26T12:34:06.012345+02:00", true)]
			[InlineData("2019-07-26T10:34:06Z", true)]
			[InlineData("2018-07-26T10:34:06Z", false)] // One year earlier
			public async Task When_configuring_header_on_date_value_should_ignore_milliseconds_and_honor_timezone(string xmlDateTime, bool expectedResult)
			{
				// Act
				_sut.Header("Last-Modified", XmlConvert.ToDateTime(xmlDateTime, XmlDateTimeSerializationMode.Utc));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
#if NETFRAMEWORK
				matchers.Should().HaveCount(1);
#else
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
#endif
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Content = new StringContent("content")
					{
						Headers =
						{
							LastModified = new DateTimeOffset(2019, 07, 26, 12, 34, 06, 012, TimeSpan.FromHours(2))
						}
					}
				})).Should().Be(expectedResult);
			}
		}

		public class Any : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("GET", true)]
			[InlineData("POST", true)]
			[InlineData("PUT", false)]
			public async Task When_configuring_any_should_match(string method, bool expectedResult)
			{
				// Act
				_sut.Any(any => any
					.Method("GET")
					.Method("POST")
				);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<AnyMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Method = new HttpMethod(method)
				})).Should().Be(expectedResult);
			}

			[Fact]
			public void Given_existing_matcher_when_adding_second_exclusive_should_not_throw()
			{
				// Act
				Action act = () => _sut.Any(any => any
					.Content("data")
					.WithoutContent()
				);

				// Assert
				act.Should().NotThrow();
			}
		}

		public class Where : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("GET", true)]
			[InlineData("POST", true)]
			[InlineData("PUT", false)]
			public async Task When_configuring_custom_expression_should_match(string method, bool expectedResult)
			{
				// Act
				_sut.Where(r => r.Method.Method == "GET" || r.Method.Method == "POST");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ExpressionMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Method = new HttpMethod(method)
				})).Should().Be(expectedResult);
			}
		}

		public class Version : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("2.0", true)]
			[InlineData("1.1", false)]
			public async Task When_configuring_version_should_match(string version, bool expectedResult)
			{
				// Act
				_sut.Version(version);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<VersionMatcher>();
				(await matchers.AnyAsync(new HttpRequestMessage
				{
					Version = new System.Version(2, 0)
				})).Should().Be(expectedResult);
			}

			[Fact]
			public void When_configuring_media_type_with_null_string_should_throw()
			{
				// Act
				Action act = () => _sut.Version((string)null);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("version");
			}

			[Fact]
			public void When_configuring_media_type_with_null_version_should_throw()
			{
				// Act
				Action act = () => _sut.Version((System.Version)null);

				// Assert
				act.Should().Throw<ArgumentNullException>().WithParamName("version");
			}

			[Fact]
			public void Given_existing_matcher_when_adding_second_exclusive_should_throw()
			{
				_sut.Version("2.0");

				// Act
				Action act = () => _sut.Version("1.0");

				// Assert
				act.Should().Throw<InvalidOperationException>();
			}
		}
	}
}