using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace HttpClientMock.HttpRequestMatchers
{
	public class ContentMatcher : IHttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static readonly Encoding DefaultEncoding = Encoding.UTF8;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Encoding _encoding;

		public ContentMatcher()
			: this(string.Empty)
		{
		}

		public ContentMatcher(string content)
			: this(content, DefaultEncoding)
		{
		}

		public ContentMatcher(string content, Encoding encoding)
			: this((encoding ?? DefaultEncoding).GetBytes(content))
		{
			_encoding = encoding ?? DefaultEncoding;
		}

		public ContentMatcher(byte[] content)
		{
			ExpectedContent = content ?? throw new ArgumentNullException(nameof(content));
		}

		public byte[] ExpectedContent { get; }

		/// <inheritdoc />
		public virtual bool IsMatch(HttpRequestMessage request)
		{
			// Use of ReadAsByteArray() will use internal buffer, so we can re-enter this method multiple times.
			// In comparison, ReadAsStream() will return the underlying stream which can only be read once.
			byte[] receivedContent = request.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			if (receivedContent == null && ExpectedContent.Length == 0)
			{
				return true;
			}

			return IsMatch(receivedContent);
		}

		protected virtual bool IsMatch(byte[] receivedContent)
		{
			return receivedContent != null && receivedContent.SequenceEqual(ExpectedContent);
		}

		public override string ToString()
		{
			if (_encoding != null)
			{
				return $"Content: {_encoding.GetString(ExpectedContent, 0, ExpectedContent.Length)}";
			}

			string msg = string.Join(",", ExpectedContent.Take(Math.Min(10, ExpectedContent.Length)).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
			return $"Content: [{msg}]";
		}
	}
}
