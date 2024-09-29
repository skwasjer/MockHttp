using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MockHttp;

/// <summary>
/// The exception thrown when a mock is configured incorrectly or no invocation is matched.
/// </summary>
[Serializable]
public class HttpMockException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMockException" />.
    /// </summary>
    internal HttpMockException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMockException" /> class using specified <paramref name="message" />.
    /// </summary>
    /// <param name="message">The exception message.</param>
    internal HttpMockException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpMockException" /> class using specified <paramref name="message" />.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    internal HttpMockException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
#if NET8_0_OR_GREATER
#pragma warning disable CA1041
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#pragma warning restore CA1041
#endif
    protected HttpMockException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
