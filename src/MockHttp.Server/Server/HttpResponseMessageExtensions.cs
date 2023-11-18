using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace MockHttp.Server;

internal static class HttpResponseMessageExtensions
{
    internal static async Task MapToFeatureAsync
    (
        this HttpResponseMessage response,
        IHttpResponseFeature responseFeature,
        IHttpResponseBodyFeature responseBodyFeature,
        CancellationToken cancellationToken)
    {
        responseFeature.StatusCode = (int)response.StatusCode;
        responseFeature.ReasonPhrase = response.ReasonPhrase;

        CopyHeaders(response.Headers, responseFeature.Headers);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (response.Content is not null)
        {
            CopyHeaders(response.Content.Headers, responseFeature.Headers);
            Stream contentStream = await response.Content.ReadAsStreamAsync(
#if NET6_0_OR_GREATER
                cancellationToken
#endif
            ).ConfigureAwait(false);
            await using ConfiguredAsyncDisposable _ = contentStream.ConfigureAwait(false);
            await contentStream.CopyToAsync(responseBodyFeature.Writer.AsStream(), 4096, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void CopyHeaders(HttpHeaders httpClientHeaders, IHeaderDictionary headers)
    {
        // ReSharper disable once UseDeconstruction
        foreach (KeyValuePair<string, IEnumerable<string>> header in httpClientHeaders)
        {
            headers[header.Key] = new StringValues(header.Value?.ToArray());
        }
    }
}
