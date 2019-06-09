using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClientMock.Language;
using HttpClientMock.Language.Flow;

namespace HttpClientMock
{
	public static class IRespondsExtensions
	{
		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function that provides the response message to return for a request.</param>
		public static IResponseResult RespondWith(this IResponds responds, Func<HttpResponseMessage> response)
		{
			return responds.RespondWith(_ => response());
		}

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function that provides the response message to return for given request.</param>
		public static IResponseResult RespondWith(this IResponds responds, Func<HttpRequestMessage, HttpResponseMessage> response)
		{
			return responds.RespondWith(request => Task.FromResult(response(request)));
		}
		
		public static IResponseResult RespondWith(this IResponds responds, HttpResponseMessage response)
		{
			return responds.RespondWith(() => response);
		}

		public static IResponseResult RespondWith(this IResponds responds, HttpStatusCode statusCode)
		{
			return responds.RespondWith(new HttpResponseMessage(statusCode));
		}
	}
}
