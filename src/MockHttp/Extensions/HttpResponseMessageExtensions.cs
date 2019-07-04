using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttp
{
	internal static class HttpResponseMessageExtensions
	{
		/// <summary>
		/// Creates a shallow clone of <see cref="HttpResponseMessage"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		/// <remarks>This does not create a true clone, because inherited messages could have extra properties, and we're not using reflection.</remarks>
		public static async Task<T> CloneResponseAsync<T>(this T response)
			where T : HttpResponseMessage, new()
		{
			if (response == default)
			{
				return default;
			}

			var clone = new T
			{
				StatusCode = response.StatusCode,
				Content = response.Content == null
					? null
					: await response.Content.CloneAsByteArrayContentAsync().ConfigureAwait(false),
				Version = new Version(response.Version.ToString()),
				ReasonPhrase = response.ReasonPhrase,
				RequestMessage = response.RequestMessage
			};

			foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
			{
				clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return clone;
		}
	}
}