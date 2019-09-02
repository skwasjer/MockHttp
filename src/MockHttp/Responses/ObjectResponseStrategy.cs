using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Responses
{
	/// <summary>
	/// Represents a base strategy that produces a mocked response based on a provided value.
	/// </summary>
	public abstract class ObjectResponseStrategy : IResponseStrategy
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectResponseStrategy"/> class.
		/// </summary>
		/// <param name="statusCode">The status code to return.</param>
		/// <param name="typeOfValue">The type of the value to return.</param>
		/// <param name="valueFactory">The value factory.</param>
		/// <param name="mediaType">The media type to return.</param>
		public ObjectResponseStrategy(HttpStatusCode statusCode, Type typeOfValue, Func<HttpRequestMessage, object> valueFactory, MediaTypeHeaderValue mediaType)
		{
			StatusCode = statusCode;
			TypeOfValue = typeOfValue ?? throw new ArgumentNullException(nameof(typeOfValue));
			ValueFactory = valueFactory;
			MediaType = mediaType;
		}

		/// <summary>
		/// Gets the status code to return.
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// Gets the type of the value to return.
		/// </summary>
		public Type TypeOfValue { get; }

		/// <summary>
		/// Gets the value factory.
		/// </summary>
		public Func<HttpRequestMessage, object> ValueFactory { get; }

		/// <summary>
		/// Gets the media type to return.
		/// </summary>
		public MediaTypeHeaderValue MediaType { get; }

		/// <inheritdoc />
		public abstract Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken);
	}
}