using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace MockHttp.Server
{
	internal class WrappedHttpRequest : HttpRequestMessage
	{
		private const int DefaultPort = 80;

		public WrappedHttpRequest(HttpRequest request)
		{
			Method = new HttpMethod(request.Method);

			var uriBuilder = new UriBuilder(
				request.Scheme,
				request.Host.Host,
				request.Host.Port ?? DefaultPort,
				request.PathBase + request.Path,
				request.QueryString.Value);

			RequestUri = uriBuilder.Uri;

			if (request.Body != null)
			{
				Content = new StreamContent(request.Body)
				{
					Headers =
					{
						ContentType = request.ContentType == null ? null : MediaTypeHeaderValue.Parse(request.ContentType),
						ContentLength = request.ContentLength
					}
				};
			}

			foreach (KeyValuePair<string, StringValues> header in request.Headers)
			{
				Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
			}
		}
	}
}
