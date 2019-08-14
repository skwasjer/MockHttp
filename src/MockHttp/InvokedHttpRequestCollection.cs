using MockHttp.Threading;

namespace MockHttp
{
	internal class InvokedHttpRequestCollection : ConcurrentCollection<IInvokedHttpRequest>, IInvokedHttpRequestCollection
	{
	}
}