using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace MockHttp.Responses
{
	/// <summary>
	/// Represents the mocked request context.
	/// </summary>
	public class MockHttpRequestContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MockHttpRequestContext"/> class.
		/// </summary>
		/// <param name="request">The request message.</param>
		public MockHttpRequestContext(HttpRequestMessage request)
		{
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}

		/// <summary>
		/// Gets the request.
		/// </summary>
		public HttpRequestMessage Request { get; }
	}
}