using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace HttpClientMock.Matchers
{
	public class ContentMatcher : ValueMatcher<byte[]>
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
			: base(content)
		{
			if (content == null)
			{
				throw new ArgumentNullException(nameof(content));
			}
		}

		/// <inheritdoc />
		public override bool IsMatch(HttpRequestMessage request)
		{
			// Use of ReadAsByteArray() will use internal buffer, so we can re-enter this method multiple times.
			// In comparison, ReadAsStream() will return the underlying stream which can only be read once.
			byte[] receivedContent = request.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			if (receivedContent == null)
			{
				return Value.Length == 0;
			}

			if (receivedContent.Length == 0 && Value.Length == 0)
			{
				return true;
			}

			return IsMatch(receivedContent);
		}

		protected virtual bool IsMatch(byte[] receivedContent)
		{
			return receivedContent.SequenceEqual(Value);
		}

		public override string ToString()
		{
			if (_encoding != null)
			{
				return $"Content: {_encoding.GetString(Value, 0, Value.Length)}";
			}

			string msg = string.Join(",", Value.Take(Math.Min(10, Value.Length)).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
			return $"Content: [{msg}]";
		}
	}
}
