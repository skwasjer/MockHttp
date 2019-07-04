using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttp
{
	internal static class HttpRequestMessageExtensions
	{
		/// <summary>
		/// Creates a shallow clone of <see cref="HttpRequestMessage"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		/// <remarks>This does not create a true clone, because inherited messages could have extra properties, and we're not using reflection.</remarks>
		public static async Task<T> CloneRequestAsync<T>(this T request)
			where T : HttpRequestMessage, new()
		{
			if (request == default)
			{
				return default;
			}

			var clone = new T
			{
				Method = request.Method,
				RequestUri = request.RequestUri,
				Content = request.Content == null
					? null
					: await request.Content.CloneAsByteArrayContentAsync().ConfigureAwait(false),
				Version = new Version(request.Version.ToString())
			};

			foreach (KeyValuePair<string, object> prop in request.Properties)
			{
				clone.Properties.Add(prop);
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
			{
				clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return clone;
		}
	}
}