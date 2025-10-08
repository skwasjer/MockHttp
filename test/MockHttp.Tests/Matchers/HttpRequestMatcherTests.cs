﻿using MockHttp.Response;

namespace MockHttp.Matchers;

public class HttpRequestMatcherTests
{
    private readonly HttpRequestMatcher _sut = Substitute.For<HttpRequestMatcher>();

    [Fact]
    public async Task Given_null_context_when_matching_it_should_throw()
    {
        MockHttpRequestContext? requestContext = null;

        // Act
        Func<Task> act = () => _sut.IsMatchAsync(requestContext!);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>()
            .WithParameterName(nameof(requestContext));
    }
}
