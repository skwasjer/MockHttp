using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IResponds : IFluentInterface
	{
		IResponseResult RespondWithAsync(Func<HttpResponseMessage> response);
		IResponseResult RespondWithAsync(Func<HttpRequestMessage, HttpResponseMessage> response);
		IResponseResult RespondWith(Func<HttpRequestMessage, Task<HttpResponseMessage>> response);
		IResponseResult RespondWith(Func<Task<HttpResponseMessage>> response);
	}
}