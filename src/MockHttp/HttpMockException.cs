using System;
using System.Diagnostics.CodeAnalysis;
#if NETFRAMEWORK
using System.Runtime.Serialization;
using System.Security;
#endif

namespace MockHttp
{
	/// <summary>
	/// The exception thrown when a mock is configured incorrectly or no invocation is matched.
	/// </summary>
#if NETFRAMEWORK
	[Serializable]
#endif
#pragma warning disable CA1032 // Implement standard exception constructors: by design.
	public class HttpMockException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpMockException"/> class using specified <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The exception message.</param>
		internal HttpMockException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpMockException"/> class using specified <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="innerException">The inner exception.</param>
		internal HttpMockException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		/// <inheritdoc />
		[ExcludeFromCodeCoverage]
		protected HttpMockException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
