using System.Net.Http.Headers;
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
		if (response.Content != null)
		{
			CopyHeaders(response.Content.Headers, responseFeature.Headers);
#if NET5_0_OR_GREATER
				await using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
			await using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
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