using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request content.
	/// </summary>
	public class ContentMatcher : ValueMatcher<byte[]>
	{
		/// <summary>
		/// The default content encoding.
		/// </summary>
		public static readonly Encoding DefaultEncoding = Encoding.UTF8;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Encoding _encoding;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMatcher"/> class.
		/// </summary>
		public ContentMatcher()
			: this(string.Empty)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMatcher"/> class using specified <paramref name="content"/> and <see cref="DefaultEncoding"/>.
		/// </summary>
		/// <param name="content">The request content to match.</param>
		public ContentMatcher(string content)
			: this(content ?? throw new ArgumentNullException(nameof(content)), DefaultEncoding)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMatcher"/> class using specified <paramref name="content"/>.
		/// </summary>
		/// <param name="content">The request content to match.</param>
		/// <param name="encoding">The content encoding.</param>
		public ContentMatcher(string content, Encoding encoding)
			: this((encoding ?? DefaultEncoding).GetBytes(content ?? throw new ArgumentNullException(nameof(content))))
		{
			_encoding = encoding ?? DefaultEncoding;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentMatcher"/> class using specified raw <paramref name="content"/>.
		/// </summary>
		/// <param name="content">The request content to match.</param>
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
			byte[] requestContent = request.Content?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			if (requestContent == null)
			{
				return Value.Length == 0;
			}

			if (requestContent.Length == 0 && Value.Length == 0)
			{
				return true;
			}

			return IsMatch(requestContent);
		}

		/// <inheritdoc />
		public override bool IsExclusive => true;

		/// <summary>
		/// Checks that the request matches the specified <paramref name="requestContent"/>.
		/// </summary>
		/// <param name="requestContent">The request content received.</param>
		/// <returns><see langword="true"/> if the content matches, <see langword="false"/> otherwise.</returns>
		protected virtual bool IsMatch(byte[] requestContent)
		{
			return requestContent.SequenceEqual(Value);
		}

		/// <inheritdoc />
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
