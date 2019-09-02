using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MockHttp.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// 
	/// </summary>
	public class FormDataMatcher : IAsyncHttpRequestMatcher
	{
		internal const string FormUrlEncodedMediaType = "application/x-www-form-urlencoded";
		internal const string MultipartFormDataMediaType = "multipart/form-data";

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<string, string> _matchQs;

		/// <summary>
		/// Initializes a new instance of the <see cref="FormDataMatcher"/> class using specified form data parameters.
		/// </summary>
		/// <param name="formData">The form data parameters.</param>
		public FormDataMatcher(IEnumerable<KeyValuePair<string, string>> formData)
		{
			if (formData == null)
			{
				throw new ArgumentNullException(nameof(formData));
			}

			_matchQs = formData.ToDictionary(v => v.Key, v => v.Value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FormDataMatcher"/> class using specified form data.
		/// </summary>
		/// <param name="urlEncodedFormData">The form data.</param>
		public FormDataMatcher(string urlEncodedFormData)
			: this(DataEscapingHelper.Parse(urlEncodedFormData ?? throw new ArgumentNullException(nameof(urlEncodedFormData)))
				.Select(v => new KeyValuePair<string, string>(v.Key, v.Value?.LastOrDefault())))
		{
		}

		/// <inheritdoc />
		public virtual async Task<bool> IsMatchAsync(HttpRequestMessage request)
		{
			if (!CanProcessContent(request.Content))
			{
				return false;
			}

			IDictionary<string, IEnumerable<string>> formData = (await GetFormDataAsync(request.Content).ConfigureAwait(false));

			// When match collection is empty, behavior is flipped, and we expect no form data parameters on request.
			if (_matchQs.Count == 0 && formData.Count > 0)
			{
				return false;
			}

			return _matchQs.All(q =>
				formData.ContainsKey(q.Key)
				 && (!formData[q.Key].Any() && string.IsNullOrEmpty(q.Value)
				|| formData[q.Key].Any(qv => string.Equals(q.Value ?? string.Empty, qv ?? string.Empty, StringComparison.Ordinal))));
		}

		/// <inheritdoc />
		public virtual bool IsExclusive => _matchQs.Count == 0;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"FormData: '{DataEscapingHelper.Format(_matchQs)}'";
		}

		private static async Task<IDictionary<string, IEnumerable<string>>> GetFormDataAsync(HttpContent content)
		{
			if (content is MultipartFormDataContent multipartFormDataContent)
			{
				var formData = new List<KeyValuePair<string, string>>();
				foreach (HttpContent httpContent in multipartFormDataContent
					.Where(c => c.Headers.ContentDisposition != null && c.Headers.ContentDisposition.DispositionType == "form-data" && !string.IsNullOrEmpty(c.Headers.ContentDisposition.Name))
				)
				{
					string key = httpContent.Headers.ContentDisposition.Name;
					bool isFileUpload = httpContent.Headers.ContentType != null && !string.IsNullOrEmpty(httpContent.Headers.ContentDisposition.FileName);
					if (isFileUpload)
					{
						// TODO: Support file uploads? Maybe using different matcher, to support (large) streams.
					}
					else
					{
						// Multipart form data is not escaped, so just read content as string.
						formData.Add(
							new KeyValuePair<string, string>(
								key,
								await httpContent.ReadAsStringAsync().ConfigureAwait(false)
							)
						);
					}
				}

				return formData
					.GroupBy(fd => fd.Key)
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Select(s => s.Value));
			}

			string rawFormData = await content.ReadAsStringAsync().ConfigureAwait(false);
			return DataEscapingHelper.Parse(rawFormData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private static bool CanProcessContent(HttpContent httpContent)
		{
			return httpContent?.Headers.ContentType != null && IsFormData(httpContent.Headers.ContentType.MediaType);
		}

		private static bool IsFormData(string mediaType)
		{
			return mediaType == FormUrlEncodedMediaType || mediaType == MultipartFormDataMediaType;
		}
	}
}
