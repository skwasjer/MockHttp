﻿using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using MockHttp.Http;
using Newtonsoft.Json;

namespace MockHttp.FluentAssertions;

public static class HttpResponseMessageAssertionsExtensions
{
    public static AndConstraint<HttpResponseMessageAssertions> HaveContentType
    (
        this HttpResponseMessageAssertions should,
        string expectedMediaType,
        string because = "",
        params object[] becauseArgs)
    {
        return should.HaveContentType(MediaTypeHeaderValue.Parse(expectedMediaType), because, becauseArgs);
    }

    public static AndConstraint<HttpResponseMessageAssertions> HaveContentType
    (
        this HttpResponseMessageAssertions should,
        MediaTypeHeaderValue expectedMediaType,
        string because = "",
        params object[] becauseArgs)
    {
        HttpResponseMessage subject = should.Subject;

        var assertionScope = (IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks;
        assertionScope
            .ForCondition(subject is not null)
            .FailWith("The subject is null.")
            .Then
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .ForCondition(subject!.Content is not null)
            .FailWith("Expected response with content {0}{reason}, but found none.")
            .Then
            .ForCondition(Equals(subject.Content?.Headers.ContentType, expectedMediaType) || (expectedMediaType.CharSet is null && Equals(subject.Content?.Headers.ContentType?.MediaType, expectedMediaType.MediaType)))
            .FailWith("Expected response with content type {0}{reason}, but found {1} instead.", expectedMediaType, subject.Content!.Headers.ContentType)
            ;

        return new AndConstraint<HttpResponseMessageAssertions>(should);
    }

    public static Task<AndConstraint<HttpResponseMessageAssertions>> HaveJsonContent<T>
    (
        this HttpResponseMessageAssertions should,
        T expectedContent,
        string because = "",
        params object[] becauseArgs)
    {
        return should.HaveContentAsync(JsonConvert.SerializeObject(expectedContent), because, becauseArgs);
    }

    public static Task<AndConstraint<HttpResponseMessageAssertions>> HaveContentAsync
    (
        this HttpResponseMessageAssertions should,
        string expectedContent,
        string because = "",
        params object[] becauseArgs)
    {
        return should.HaveContentAsync(new ByteArrayContent(Encoding.UTF8.GetBytes(expectedContent)), because, becauseArgs);
    }

    public static async Task<AndConstraint<HttpResponseMessageAssertions>> HaveContentAsync
    (
        this HttpResponseMessageAssertions should,
        HttpContent expectedContent,
        string because = "",
        params object[] becauseArgs)
    {
        if (expectedContent is null)
        {
            throw new ArgumentNullException(nameof(expectedContent));
        }

        if (expectedContent.Headers.ContentType is not null)
        {
            should.HaveContentType(expectedContent.Headers.ContentType);
        }

        HttpResponseMessage subject = should.Subject;

        ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks)
            .ForCondition(subject is not null)
            .FailWith("The subject is null.")
            .Then
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .ForCondition(subject!.Content is not null)
            .FailWith("Expected response with content {reason}, but has no content.");

        byte[] currentContentBytes = await subject.Content!.ReadAsByteArrayAsync();
        byte[] expectedContentBytes = await expectedContent.ReadAsByteArrayAsync();

        ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks)
            .ForCondition(currentContentBytes.SequenceEqual(expectedContentBytes))
            // Using UTF-8 for fail msg, but this will not produce correct result for other encodings or binary responses.
            // Since this is a private test helper, we accept this for now.
            .FailWith("Expected response with content {0} to match {1}{reason}, but it did not.", Encoding.UTF8.GetString(currentContentBytes), Encoding.UTF8.GetString(expectedContentBytes));

        return new AndConstraint<HttpResponseMessageAssertions>(should);
    }

    public static AndConstraint<HttpResponseMessageAssertions> HaveHeader
    (
        this HttpResponseMessageAssertions should,
        string key,
        string value,
        string because = "",
        params object[] becauseArgs)
    {
        return should.HaveHeader(key, value is null ? null : new[] { value }, because, becauseArgs);
    }

    public static AndConstraint<HttpResponseMessageAssertions> HaveHeader
    (
        this HttpResponseMessageAssertions should,
        string key,
        IEnumerable<string> values,
        string because = "",
        params object[] becauseArgs)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        HttpResponseMessage subject = should.Subject;
        var expectedHeader = new KeyValuePair<string, IEnumerable<string>>(key, values);
        var equalityComparer = new HttpHeaderEqualityComparer();

        var assertionScope = (IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks;
        assertionScope
            .ForCondition(subject is not null)
            .FailWith("The subject is null.")
            .Then
            .ForCondition(subject!.Headers.Contains(key) || (subject.Content?.Headers.Contains(key) ?? false))
            .FailWith("Expected response to have header {0}{reason}, but found none.", key)
            .Then
            .ForCondition(
                subject.Headers.Any(h => equalityComparer.Equals(h, expectedHeader))
             || (subject.Content?.Headers.Any(h => equalityComparer.Equals(h, expectedHeader)) ?? false))
            .FailWith(() =>
            {
                if (subject.Headers.TryGetValues(key, out IEnumerable<string>? headerValues))
                {
                    return new FailReason("Expected response to have header {0} with value {1}{reason}, but found value {2}.", key, values, headerValues);
                }

                if (!subject.Content!.Headers.TryGetValues(key, out headerValues))
                {
                    headerValues = new List<string>();
                }

                return new FailReason("Expected response to have header {0} with value {1}{reason}, but found value {2}.", key, values, headerValues);
            })
            ;

        return new AndConstraint<HttpResponseMessageAssertions>(should);
    }
}