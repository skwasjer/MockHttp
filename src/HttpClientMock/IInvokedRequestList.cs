using System.Collections.Generic;

namespace HttpClientMock
{
	public interface IInvokedRequestList : IReadOnlyList<IMockedHttpRequest>
	{
		/// <summary>
		/// Resets the invoked requests.
		/// </summary>
		void Reset();
	}
}