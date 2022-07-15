#nullable enable
using System.Net.Http.Headers;
using System.Text;
using MockHttp.Language.Response;
using Moq;
using Xunit;

namespace MockHttp.Language.Flow.Response;

public class NullBuilderTests
{
        [Theory]
        [MemberData(nameof(TestCases))]
        public void Given_null_argument_when_executing_method_it_should_throw(params object[] args)
        {
            NullArgumentTest.Execute(args);
        }

        public static IEnumerable<object[]> TestCases()
        {
            var responseBuilderImpl = new ResponseBuilder();
            IResponseBuilder responseBuilder = Mock.Of<IResponseBuilder>();
            IWithContent withContent = Mock.Of<IWithContent>();
            IWithStatusCode withStatusCode = Mock.Of<IWithStatusCode>();
            IWithContentType withContentType = Mock.Of<IWithContentType>();
            IWithHeaders withHeaders = Mock.Of<IWithHeaders>();

            DelegateTestCase[] testCases =
            {
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.StatusCode,
                    withStatusCode,
                    200,
                    (string?)null
                ),
                DelegateTestCase.Create(
                    responseBuilderImpl.Body,
                    () => Task.FromResult<HttpContent>(new StringContent(""))
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    new StringContent("")
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    "body text",
                    (Encoding?)null
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    Array.Empty<byte>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    Array.Empty<byte>(),
                    0,
                    0
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    Stream.Null
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Body,
                    withContent,
                    () => Stream.Null
                ),
                DelegateTestCase.Create(
                    responseBuilderImpl.ContentType,
                    new MediaTypeHeaderValue("text/plain")
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.ContentType,
                    withContentType,
                    "text/plain",
                    (Encoding?)null
                ),
                DelegateTestCase.Create(
                    responseBuilderImpl.Headers,
                    Enumerable.Empty<KeyValuePair<string, IEnumerable<string?>>>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Header,
                    withHeaders,
                    "header-name",
                    (string?[])null!
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Header,
                    withHeaders,
                    new KeyValuePair<string, string>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Header,
                    withHeaders,
                    new KeyValuePair<string, object>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Header,
                    withHeaders,
                    new KeyValuePair<string, IEnumerable<string>>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Header,
                    withHeaders,
                    new KeyValuePair<string, IEnumerable<object>>()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.ClientTimeout,
                    responseBuilder,
                    (TimeSpan?)null
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.ServerTimeout,
                    responseBuilder,
                    (TimeSpan?)null
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Latency,
                    responseBuilder,
                    NetworkLatency.FourG()
                ),
                DelegateTestCase.Create(
                    ResponseBuilderExtensions.Latency,
                    responseBuilder,
                    () => NetworkLatency.FourG()
                )
            };

            return testCases.SelectMany(tc => tc.GetNullArgumentTestCases());
        }
}
#nullable restore
