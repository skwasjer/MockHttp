using System;
using System.ComponentModel;
using System.Net.Http;
using HttpClientMock.Language.Flow;

namespace HttpClientMock.Language
{
	/// <summary>
	/// Defines the <c>Callback</c> verb and overloads for callbacks.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICallback : IFluentInterface
	{
		/// <summary>
		/// Specifies a callback to invoke when the request is sent.
		/// </summary>
		/// <param name="callback">The callback to invoke.</param>
		ICallbackResult Callback(Action callback);

		/// <summary>
		/// Specifies a callback to invoke when the request is sent.
		/// </summary>
		/// <param name="callback">The callback to invoke.</param>
		ICallbackResult Callback(Action<HttpRequestMessage> callback);
	}
}