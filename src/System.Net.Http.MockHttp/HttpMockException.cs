#if NETFRAMEWORK
using System.Runtime.Serialization;
using System.Security;
#endif

namespace System.Net.Http.MockHttp
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
		protected HttpMockException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		///// <summary>Sets the <see cref="SerializationInfo" /> with information about the exception.</summary>
		///// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
		///// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination. </param>
		///// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is a null reference (<see langword="Nothing" /> in Visual Basic). </exception>
		//[SecurityCritical]
		//public override void GetObjectData(SerializationInfo info, StreamingContext context)
		//{
		//	base.GetObjectData(info, context);
		//}
#endif
	}
}
