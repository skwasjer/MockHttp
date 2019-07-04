﻿using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using MockHttp.Language.Flow;

namespace MockHttp.Language
{
	/// <summary>
	/// Defines the <c>Responds</c> verb and overloads for configuring responses.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IResponds : IFluentInterface
	{
		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function returning an awaitable that when completed provides the response message to return for a request.</param>
		IResponseResult Respond(Func<Task<HttpResponseMessage>> response);

		/// <summary>
		/// Specifies a function that returns the response for a request.
		/// </summary>
		/// <param name="response">The function returning an awaitable that when completed provides the response message to return for given request.</param>
		IResponseResult Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> response);
	}
}