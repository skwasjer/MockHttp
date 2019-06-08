using System;
using System.ComponentModel;
using System.Net.Http;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ICallback : IFluentInterface
	{
		ICallbackResult Callback(Action callback);
		ICallbackResult Callback(Action<HttpRequestMessage> callback);
	}
}