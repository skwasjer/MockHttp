using System.Collections.Generic;

namespace HttpClientMock
{
	public interface IInvokedRequestCollection : IReadOnlyList<IMockedHttpRequest>
	{
		/// <summary>
		/// Resets the invoked requests.
		/// </summary>
		void Reset();
	}
}