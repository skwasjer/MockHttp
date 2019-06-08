using System.Collections.Generic;

namespace HttpClientMock
{
	public interface IInvokedHttpRequestCollection : IReadOnlyList<IInvokedHttpRequest>
	{
		/// <summary>
		/// Resets the invoked requests.
		/// </summary>
		void Reset();
	}
}