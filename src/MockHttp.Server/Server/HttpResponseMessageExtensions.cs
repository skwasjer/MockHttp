using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace MockHttp.Server
{
	internal static class HttpResponseMessageExtensions
	{
		internal static async Task MapToFeatureAsync(this HttpResponseMessage response, IHttpResponseFeature responseFeature, CancellationToken cancellationToken)
		{
			responseFeature.StatusCode = (int)response.StatusCode;
			responseFeature.ReasonPhrase = response.ReasonPhrase;

			CopyHeaders(response.Headers, responseFeature.Headers);
			if (response.Content != null)
			{
				CopyHeaders(response.Content.Headers, responseFeature.Headers);
				// ReSharper disable once UseAwaitUsing
				using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
				await contentStream.CopyToAsync(responseFeature.Body, 4096, cancellationToken).ConfigureAwait(false);
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
}
