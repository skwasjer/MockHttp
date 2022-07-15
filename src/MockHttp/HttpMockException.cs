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
    protected HttpMockException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
