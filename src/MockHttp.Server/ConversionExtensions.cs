using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace MockHttp.Server
{
	internal static class ConversionExtensions
	{
		public const int DefaultPort = 80;

		public static HttpRequestMessage AsHttpRequestMessage(this HttpRequest request)
		{
			var uriBuilder = new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? DefaultPort, request.GetEncodedPathAndQuery());
			var newRequest = new HttpRequestMessage(new HttpMethod(request.Method), uriBuilder.Uri);

			if (request.Body != null)
			{
				newRequest.Content = new StreamContent(request.Body)
				{
					Headers =
					{
						ContentType = request.ContentType == null ? null : new MediaTypeHeaderValue(request.ContentType),
						ContentLength = request.ContentLength
					}
				};
			}

			foreach (KeyValuePair<string, StringValues> header in request.Headers)
			{
				newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
			}

			return newRequest;
		}

		public static async Task MapToResponseFeatureAsync(this HttpResponseMessage response, IHttpResponseFeature responseFeature, CancellationToken cancellationToken)
		{
			responseFeature.StatusCode = (int)response.StatusCode;
			responseFeature.ReasonPhrase = response.ReasonPhrase;

			if (response.Content != null)
			{
				foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
				{
					responseFeature.Headers[header.Key] = new StringValues(header.Value?.ToArray());
				}

				Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
				await contentStream.CopyToAsync(responseFeature.Body, 4096, cancellationToken).ConfigureAwait(false);
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
			{
				responseFeature.Headers[header.Key] = new StringValues(header.Value?.ToArray());
			}
		}
	}
}
