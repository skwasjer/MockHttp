using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

		public class Url : RequestMatchingExtensionsTests
		{
			[Fact]
			public void When_configuring_url_should_match()
			{
				// Act
				_sut.Url("http://127.0.0.1/");
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<RequestUriMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1/")
				}).Should().BeTrue();
			}
		}

		public class QueryString : RequestMatchingExtensionsTests
		{
			[Fact]
			public void When_configuring_query_string_with_null_value_should_match()
			{
				// Act
				_sut.QueryString("key", (string)null);
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key")
				}).Should().BeTrue();
			}

			[Fact]
			public void When_configuring_query_string_with_empty_value_should_match()
			{
				// Act
				_sut.QueryString("key", string.Empty);
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1?key=")
				}).Should().BeTrue();
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

			[Theory]
			[InlineData("?")]
			[InlineData("")]
			public void When_configuring_without_query_string_should_match(string queryString)
			{
				// Act
				_sut.WithoutQueryString();
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<QueryStringMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					RequestUri = new Uri("http://127.0.0.1" + queryString)
				}).Should().BeTrue();
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
			public void When_configuring_httpMethod_should_match(string httpMethod, bool expectedResult)
			{
				// Act
				_sut.Method(httpMethod);
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<HttpMethodMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					Method = new HttpMethod("POST")
				}).Should().Be(expectedResult);
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
			public void When_configuring_contentType_should_match(string mediaType, bool expectedResult)
			{
				// Act
				_sut.ContentType(mediaType);
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<MediaTypeHeaderMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					Content = new StringContent(string.Empty, Encoding.ASCII, "text/plain")
				}).Should().Be(expectedResult);
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

		public class Any : RequestMatchingExtensionsTests
		{
			[Theory]
			[InlineData("GET", true)]
			[InlineData("POST", true)]
			[InlineData("PUT", false)]
			public void When_configuring_any_should_match(string method, bool expectedResult)
			{
				// Act
				_sut.Any(any => any
					.Method("GET")
					.Method("POST")
				);
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<AnyMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					Method = new HttpMethod(method)
				}).Should().Be(expectedResult);
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
			public void When_configuring_custom_expression_should_match(string method, bool expectedResult)
			{
				// Act
				_sut.Where(r => r.Method.Method == "GET" || r.Method.Method == "POST");
				IReadOnlyCollection<HttpRequestMatcher> matchers = _sut.Build();

				// Assert
				matchers.Should().HaveCount(1).And.AllBeOfType<ExpressionMatcher>();
				matchers.Any(new HttpRequestMessage
				{
					Method = new HttpMethod(method)
				}).Should().Be(expectedResult);
			}
		}
	}
}