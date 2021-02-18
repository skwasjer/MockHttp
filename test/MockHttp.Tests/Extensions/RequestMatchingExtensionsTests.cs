using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Matchers;
using MockHttp.Responses;
using Moq;
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
				var request = new HttpRequestMessage { RequestUri = new Uri("http://127.0.0.1/") };

				// Act
				_sut.RequestUri("http://127.0.0.1/");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<RequestUriMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Fact]
			public async Task When_configuring_requestUri_as_uri_should_match()
			{
				var uri = new Uri("http://127.0.0.1/");
				var request = new HttpRequestMessage { RequestUri = new Uri("http://127.0.0.1/") };

				// Act
				_sut.RequestUri(uri);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<RequestUriMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}
		}

		public class QueryString : RequestMatchingExtensionsTests
		{
			[Fact]
			public async Task When_configuring_query_string_with_null_value_should_match()
			{
				var request = new HttpRequestMessage { RequestUri = new Uri("http://127.0.0.1?key") };

				// Act
				_sut.QueryString("key", (string)null);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Fact]
			public async Task When_configuring_query_string_with_empty_value_should_match()
			{
				var request = new HttpRequestMessage { RequestUri = new Uri("http://127.0.0.1?key=") };

				// Act
				_sut.QueryString("key", string.Empty);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Theory]
			[InlineData("?key1=value1", true)]
			[InlineData("key2=value2&key1=value1", true)]
			[InlineData("http://127.0.0.1?key1=value1&key2=value2", true)]
			[InlineData("?other=value1", false)]
			public async Task When_configuring_query_string_should_match(string queryString, bool expectedResult)
			{
				var request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key1=value1&key2=value2")
				};

				// Act
				_sut.QueryString(queryString);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
				var request = new HttpRequestMessage { RequestUri = new Uri("http://127.0.0.1" + queryString) };

				// Act
				_sut.WithoutQueryString();
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
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
				var request = new HttpRequestMessage { Method = new HttpMethod("POST") };

				// Act
				_sut.Method(httpMethod);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpMethodMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
				var request = new HttpRequestMessage
				{
					Content = new StringContent(string.Empty, Encoding.ASCII, "text/plain")
				};

				// Act
				_sut.ContentType(mediaType);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<MediaTypeHeaderMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
		}

		public class FormData : RequestMatchingExtensionsTests
		{
			[Fact]
			public async Task When_configuring_formData_with_null_value_should_match()
			{
				var request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1"),
					Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("key", null)
					})
				};

				// Act
				_sut.FormData("key", string.Empty);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<FormDataMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Fact]
			public async Task When_configuring_formData_with_empty_value_should_match()
			{
				var request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1"),
					Content = new FormUrlEncodedContent(new[]
					{
						new KeyValuePair<string, string>("key", string.Empty)
					})
				};

				// Act
				_sut.FormData("key", string.Empty);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<FormDataMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Theory]
			[InlineData("key1=value1", true)]
			[InlineData("key2=value2&key1=value1", true)]
			[InlineData("key1=value1&key2=value2", true)]
			[InlineData("other=value1", false)]
			public async Task When_configuring_formData_should_match(string urlEncodedFormData, bool expectedResult)
			{
				var request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1"),
					Content = new ByteArrayContent(Encoding.UTF8.GetBytes("key1=value1&key2=value2"))
					{
						Headers =
						{
							ContentType = new MediaTypeHeaderValue(FormDataMatcher.FormUrlEncodedMediaType)
						}
					}
				};

				// Act
				_sut.FormData(urlEncodedFormData);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<FormDataMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("key1=value1", true)]
			[InlineData("key2=éôxÄ&key1=value1", true)]
			[InlineData("key1=value1&key2=éôxÄ", true)]
			[InlineData("other=value1", false)]
			public async Task When_configuring_formData_should_match_multipart(string urlEncodedFormData, bool expectedResult)
			{
				var content = new MultipartFormDataContent
				{
					{ new ByteArrayContent(Encoding.UTF8.GetBytes("value1")), "key1" },
					{ new ByteArrayContent(Encoding.UTF8.GetBytes("éôxÄ")), "key2" },
					{ new StringContent("file content 1", Encoding.UTF8, "text/plain"), "file1", "file1.txt" }
				};
				var request = new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1"),
					Content = content
				};

				// Act
				_sut.FormData(urlEncodedFormData);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<FormDataMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Fact]
			public void When_configuring_empty_formData_should_throw()
			{
				string urlEncodedFormData = string.Empty;

				// Act
				Action act = () => _sut.FormData(urlEncodedFormData);

				// Assert
				act.Should()
					.Throw<ArgumentException>()
					.WithParamName(nameof(urlEncodedFormData))
					.WithMessage("Specify the url encoded form data*");
			}

			[Fact]
			public void When_configuring_null_formData_should_throw()
			{
				string urlEncodedFormData = null;

				// Act
				// ReSharper disable once ExpressionIsAlwaysNull
				Action act = () => _sut.FormData(urlEncodedFormData);

				// Assert
				act.Should()
					.Throw<ArgumentException>()
					.WithParamName(nameof(urlEncodedFormData))
					.WithMessage("Specify the url encoded form data*");
			}

			[Fact]
			public void Given_formData_matcher_is_already_added_when_configuring_another_should_not_throw()
			{
				_sut.FormData("key", "value");

				// Act
				Action act = () => _sut.FormData("key1", "value1");

				// Assert
				act.Should().NotThrow();
			}
		}

		public class Content : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("content", true)]
			[InlineData("more content", false)]
			public async Task When_configuring_content_should_match(string content, bool expectedResult)
			{
				var request = new HttpRequestMessage { Content = new StringContent("content") };

				// Act
				_sut.Content(content);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("utf-8", true)]
			[InlineData("us-ascii", false)]
			public async Task When_configuring_content_with_encoding_should_match(string encoding, bool expectedResult)
			{
				var request = new HttpRequestMessage { Content = new StringContent("straße", Encoding.UTF8) };

				// Act
				_sut.Content("straße", Encoding.GetEncoding(encoding));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("content", true)]
			[InlineData("more content", false)]
			public async Task When_configuring_content_with_stream_should_match(string content, bool expectedResult)
			{
				byte[] data = Encoding.UTF8.GetBytes(content);
				var request = new HttpRequestMessage { Content = new StringContent("content") };
				using var ms = new MemoryStream(data);
				// Act
				_sut.Content(ms);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
				var request = new HttpRequestMessage { Content = new StringContent("one two three") };

				// Act
				_sut.PartialContent(content);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("utf-8", true)]
			[InlineData("us-ascii", false)]
			public async Task When_configuring_partial_content_with_encoding_should_match(string encoding, bool expectedResult)
			{
				var request = new HttpRequestMessage { Content = new StringContent("straße", Encoding.UTF8) };

				// Act
				_sut.PartialContent("ß", Encoding.GetEncoding(encoding));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("one two three", true)]
			[InlineData("four", false)]
			[InlineData("two", true)]
			public async Task When_configuring_partial_content_with_stream_should_match(string content, bool expectedResult)
			{
				byte[] data = Encoding.UTF8.GetBytes(content);
				var request = new HttpRequestMessage { Content = new StringContent("one two three") };
				using var ms = new MemoryStream(data);
				// Act
				_sut.PartialContent(ms);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<PartialContentMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Fact]
			public void Given_partial_content_matcher_is_already_added_when_configuring_should_not_throw()
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
				var request = new HttpRequestMessage { Content = new StringContent("one two three", Encoding.UTF8) };

				// Act
				_sut.PartialContent("one");
				_sut.PartialContent("three");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
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
				var request = new HttpRequestMessage
				{
					Content = new StringContent("content")
					{
						Headers =
						{
							LastModified = new DateTimeOffset(2019, 07, 26, 12, 34, 06, 012, TimeSpan.FromHours(2))
						}
					}
				};

				// Act
				_sut.Header("Last-Modified", XmlConvert.ToDateTime(xmlDateTime, XmlDateTimeSerializationMode.Utc));
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
#if NETFRAMEWORK
				matchers.Should().HaveCount(1);
#else
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
#endif
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}

			[Theory]
			[InlineData("Cache-Control", true)]
			[InlineData("Content-Type", false)]
			public async Task When_configuring_name_should_match(string name, bool expectedResult)
			{
				var request = new HttpRequestMessage
				{
					Headers =
					{
						CacheControl =
							new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromHours(2) }
					}
				};

				// Act
				_sut.Header(name);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
#if NETFRAMEWORK
				matchers.Should().HaveCount(1);
#else
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
#endif
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
				var request = new HttpRequestMessage { Method = new HttpMethod(method) };

				// Act
				_sut.Any(any => any
					.Method("GET")
					.Method("POST")
				);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<AnyMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
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
				var request = new HttpRequestMessage { Method = new HttpMethod(method) };

				// Act
				_sut.Where(r => r.Method.Method == "GET" || r.Method.Method == "POST");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ExpressionMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().Be(expectedResult);
			}
		}

		public class Version : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("2.0", true)]
			[InlineData("1.1", false)]
			public async Task When_configuring_version_should_match(string version, bool expectedResult)
			{
				var request = new HttpRequestMessage
				{
					Version = new System.Version(2, 0)
				};

				// Act
				_sut.Version(version);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<VersionMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request)))
					.Should()
					.Be(expectedResult);
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

		public class NullArgumentTests : RequestMatchingExtensionsTests
		{
			[Theory]
			[MemberData(nameof(TestCases))]
			public void Given_null_argument_when_executing_method_it_should_throw(params object[] args)
			{
				NullArgumentTest.Execute(args);
			}

			public static IEnumerable<object[]> TestCases()
			{
				var streamMock = new Mock<Stream> { CallBase = true };
				streamMock.SetReturnsDefault(true);
				Uri uri = new Uri("http://0.0.0.0");
				var instance = new RequestMatching();

				DelegateTestCase[] testCases =
				{
					DelegateTestCase.Create(
						RequestMatchingExtensions.RequestUri,
						instance,
						uri.ToString()),
					DelegateTestCase.Create(
						RequestMatchingExtensions.RequestUri,
						instance,
						uri),
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						"key",
						(string)null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						"key",
						(IEnumerable<string>)null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						"key",
						(string[])null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						new Dictionary<string, IEnumerable<string>>()),
#if !NETCOREAPP1_1
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						new NameValueCollection()),
#endif
					DelegateTestCase.Create(
						RequestMatchingExtensions.QueryString,
						instance,
						"?test"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.WithoutQueryString,
						instance),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Method,
						instance,
						"GET"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Method,
						instance,
						HttpMethod.Get),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						(string)null,
						false),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						1),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						true),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						DateTime.Now),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						DateTimeOffset.Now),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						Enumerable.Empty<string>()),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Header,
						instance,
						"header",
						new string[0]),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Headers,
						instance,
						"header: value"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Headers,
						instance,
						new Dictionary<string, string>()),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Headers,
						instance,
						new Dictionary<string, IEnumerable<string>>()),
					DelegateTestCase.Create(
						RequestMatchingExtensions.ContentType,
						instance,
						"text/plain"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.ContentType,
						instance,
						"text/plain",
						Encoding.UTF8),
					DelegateTestCase.Create(
						RequestMatchingExtensions.ContentType,
						instance,
						new MediaTypeHeaderValue("text/plain")),
					DelegateTestCase.Create(
						RequestMatchingExtensions.FormData,
						instance,
						"key",
						(string)null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.FormData,
						instance,
						new Dictionary<string, string>()),
#if !NETCOREAPP1_1
					DelegateTestCase.Create(
						RequestMatchingExtensions.FormData,
						instance,
						new NameValueCollection()),
#endif
					DelegateTestCase.Create(
						RequestMatchingExtensions.FormData,
						instance,
						new Dictionary<string, IEnumerable<string>>()),
					DelegateTestCase.Create(
						RequestMatchingExtensions.FormData,
						instance,
						"key=value"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Content,
						instance,
						"content"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Content,
						instance,
						"content",
						(Encoding)null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Content,
						instance,
						Encoding.UTF8.GetBytes("content")),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Content,
						instance,
						streamMock.Object),
					DelegateTestCase.Create(
						RequestMatchingExtensions.WithoutContent,
						instance),
					DelegateTestCase.Create(
						RequestMatchingExtensions.PartialContent,
						instance,
						"partial content"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.PartialContent,
						instance,
						"partial content",
						(Encoding)null),
					DelegateTestCase.Create(
						RequestMatchingExtensions.PartialContent,
						instance,
						Encoding.UTF8.GetBytes("partial content")),
					DelegateTestCase.Create(
						RequestMatchingExtensions.PartialContent,
						instance,
						streamMock.Object),
					DelegateTestCase.Create<RequestMatching, Action<RequestMatching>, RequestMatching>(
						RequestMatchingExtensions.Any,
						instance,
						_ => { }),
					DelegateTestCase.Create<RequestMatching, Expression<Func<HttpRequestMessage, bool>>, RequestMatching>(
						RequestMatchingExtensions.Where,
						instance,
						_ => true),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Version,
						instance,
						"1.0"),
					DelegateTestCase.Create(
						RequestMatchingExtensions.Version,
						instance,
						System.Version.Parse("1.0"))
				};

				return testCases.SelectMany(tc => tc.GetNullArgumentTestCases());
			}
		}

		public class Authentication : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("token")]
			[InlineData("some-other-long-token")]
			public async Task When_configuring_any_bearer_token_should_match(string token)
			{
				var request = new HttpRequestMessage { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) } };

				// Act
				_sut.BearerToken();
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Fact]
			public async Task Given_request_does_not_have_authorization_header_when_configuring_any_bearer_token_should_not_match()
			{
				var request = new HttpRequestMessage();

				// Act
				_sut.BearerToken();
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeFalse();
			}

			[Fact]
			public async Task When_configuring_bearer_token_should_match()
			{
				const string token = "token";
				var request = new HttpRequestMessage { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) } };

				// Act
				_sut.BearerToken(token);
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeTrue();
			}

			[Fact]
			public async Task Given_request_does_not_have_authorization_header_when_configuring_bearer_token_should_not_match()
			{
				var request = new HttpRequestMessage();

				// Act
				_sut.BearerToken("token");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeFalse();
			}

			[Fact]
			public async Task Given_request_has_different_authorization_header_when_configuring_bearer_token_should_not_match()
			{
				var request = new HttpRequestMessage { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", "other-token") } };

				// Act
				_sut.BearerToken("token");
				IReadOnlyCollection<IAsyncHttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpHeadersMatcher>();
				(await matchers.AnyAsync(new MockHttpRequestContext(request))).Should().BeFalse();
			}
		}
	}
}
