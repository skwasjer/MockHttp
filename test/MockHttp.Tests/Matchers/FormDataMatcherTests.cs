using System.Net.Http.Headers;
using System.Text;
using MockHttp.Http;
using MockHttp.Responses;

namespace MockHttp.Matchers;

public class FormDataMatcherTests
{
    [Theory]
    [InlineData("key", "key", null)]
    [InlineData("key=", "key", "")]
    [InlineData("key=value", "key", "value")]
    [InlineData("key1=value1&key2=value2", "key2", "value2")]
    [InlineData("key=value1&key=value2", "key", "value1")]
    [InlineData("key1=value1&%C3%A9%C3%B4x%C3%84=%24%25%5E%26*&key2=value", "éôxÄ", "$%^&*")]
    public async Task Given_formData_equals_expected_formData_when_matching_should_match(string formData, string expectedKey, string? expectedValue)
    {
        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/" + formData),
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(formData)))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MediaTypes.FormUrlEncoded)
                }
            }
        };

        var sut = new FormDataMatcher(new[]
        {
            new KeyValuePair<string, IEnumerable<string>>(
                expectedKey,
                expectedValue is null
                    ? null!
                    : new[] { expectedValue })
        });

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("key=value")]
    [InlineData("key1=value1&key2=value2")]
    [InlineData("key=value1&key=value2")]
    public async Task Given_formData_does_not_equal_expected_formData_when_matching_should_not_match(string formData)
    {
        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/"),
            Content = new StringContent(
                formData,
                Encoding.UTF8,
                MediaTypes.FormUrlEncoded
            )
        };

        var sut = new FormDataMatcher(new[] { new KeyValuePair<string, IEnumerable<string>>("key_not_in_formdata", null!) });

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
    }

    [Fact]
    public async Task Given_formData_and_no_expected_formData_when_matching_should_not_match()
    {
        var sut = new FormDataMatcher("");

        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/"),
            Content = new StringContent(
                "unexpected=formdata",
                Encoding.UTF8,
                MediaTypes.FormUrlEncoded
            )
        };

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse("no form data was expected");
    }

    [Fact]
    public void Given_null_formData_when_creating_matcher_should_throw()
    {
        string? urlEncodedFormData = null;

        // Act
        Func<FormDataMatcher> act = () => new FormDataMatcher(urlEncodedFormData!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(urlEncodedFormData));
    }

    [Fact]
    public void Given_null_parameters_when_creating_matcher_should_throw()
    {
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? formData = null;

        // Act
        Func<FormDataMatcher> act = () => new FormDataMatcher(formData!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(formData));
    }

    [Fact]
    public void When_formatting_should_return_human_readable_representation()
    {
        const string expectedText = "FormData: 'key=value1'";
        var sut = new FormDataMatcher("key=value1");

        // Act
        string displayText = sut.ToString();

        // Assert
        displayText.Should().Be(expectedText);
    }

    [Fact]
    public async Task Given_multiPart_formData_equals_expected_formData_when_matching_should_match()
    {
        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(Encoding.UTF8.GetBytes("value1")), "key1" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("éôxÄ")), "key2" },
            { new StringContent("file content 1", Encoding.UTF8, MediaTypes.PlainText), "file1", "file1.txt" }
        };

        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/"),
            Content = content
        };

        var sut = new FormDataMatcher(new[]
        {
            new KeyValuePair<string, IEnumerable<string>>("key1", new[] { "value1" }),
            new KeyValuePair<string, IEnumerable<string>>("key2", new[] { "éôxÄ" })
        });

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeTrue();
    }

    [Fact]
    public async Task Given_multiPart_formData_does_not_equal_expected_formData_when_matching_should_not_match()
    {
        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(Encoding.UTF8.GetBytes("value1")), "key1" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("éôxÄ")), "key2" },
            { new StringContent("file content 1", Encoding.UTF8, MediaTypes.PlainText), "file1", "file1.txt" }
        };

        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/"),
            Content = content
        };

        var sut = new FormDataMatcher(new[]
        {
            new KeyValuePair<string, IEnumerable<string>>("key_not_in_formdata", null!)
        });

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
    }

    [Fact]
    public async Task Given_null_context_when_matching_it_should_throw()
    {
        var sut = new FormDataMatcher(new List<KeyValuePair<string, IEnumerable<string>>>());
        MockHttpRequestContext? requestContext = null;

        // Act
        Func<Task> act = () => sut.IsMatchAsync(requestContext!);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }

    [Theory]
    [MemberData(nameof(NonFormDataContentTestCases))]
    public async Task Given_that_content_is_not_formData_when_matching_it_should_return_false(HttpContent content)
    {
        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("http://localhost/"),
            Content = content
        };

        var sut = new FormDataMatcher(new[]
        {
            new KeyValuePair<string, IEnumerable<string>>("key", new [] { "value" })
        });

        // Act & assert
        (await sut.IsMatchAsync(new MockHttpRequestContext(request))).Should().BeFalse();
    }

    public static IEnumerable<object?[]> NonFormDataContentTestCases()
    {
        yield return new object?[] { null };
        yield return new object[] { new StringContent("") };
        yield return new object[] { new ByteArrayContent(Array.Empty<byte>()) };
    }
}
