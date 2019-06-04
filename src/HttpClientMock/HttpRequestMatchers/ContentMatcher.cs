using System;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace HttpClientMock.HttpRequestMatchers
{
	public class ContentMatcher : IHttpRequestMatcher
	{
		public ContentMatcher()
			: this(string.Empty)
		{
		}

		public ContentMatcher(string content)
			: this(content, Encoding.UTF8)
		{
		}

		public ContentMatcher(string content, Encoding encoding)
			: this(encoding?.GetBytes(content))
		{
		}

		public ContentMatcher(byte[] content)
		{
			ExpectedContent = content ?? throw new ArgumentNullException(nameof(content));
		}

		public byte[] ExpectedContent { get; }

		/// <inheritdoc />
		public virtual bool IsMatch(HttpRequestMessage request)
		{
			byte[] receivedContent = request.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			if (receivedContent == null && ExpectedContent.Length == 0)
			{
				return true;
			}

			return IsMatch(receivedContent);
		}

		protected virtual bool IsMatch(byte[] receivedContent)
		{
			return ExpectedContent.Length == receivedContent?.Length
			 && ExpectedContent
					.TakeWhile((b, index) => receivedContent[index] == b)
					.Count() == ExpectedContent.Length;
		}
	}
}
