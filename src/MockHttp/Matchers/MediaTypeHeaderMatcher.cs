using System.Net.Http.Headers;
using MockHttp.Responses;

namespace MockHttp.Matchers;

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
		if (headerValue is null)
		{
			throw new ArgumentNullException(nameof(headerValue));
		}
	}

	/// <inheritdoc />
	public override bool IsMatch(MockHttpRequestContext requestContext)
	{
		if (requestContext is null)
		{
			throw new ArgumentNullException(nameof(requestContext));
		}

		return requestContext.Request.Content?.Headers.ContentType?.Equals(Value) ?? false;
	}

	/// <inheritdoc />
	public override bool IsExclusive => true;

	/// <inheritdoc />
	public override string ToString()
	{
		return $"MediaType: {Value}";
	}
}