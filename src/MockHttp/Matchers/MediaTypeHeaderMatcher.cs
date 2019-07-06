using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the media type header.
	/// </summary>
	public class MediaTypeHeaderMatcher : ValueMatcher<MediaTypeHeaderValue>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MediaTypeHeaderMatcher"/> class.
		/// </summary>
		public MediaTypeHeaderMatcher(MediaTypeHeaderValue headerValue)
			: base(headerValue)
		{
			if (headerValue == null)
			{
				throw new ArgumentNullException(nameof(headerValue));
			}
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			return request.Content?.Headers.ContentType?.Equals(Value) ?? false;
		}

		/// <inheritdoc />
		public override bool IsExclusive => true;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ContentType: {Value}";
		}
	}
}