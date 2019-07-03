using System.Collections.Generic;

namespace MockHttp
{
	/// <summary>
	/// Represents a collection of invoked HTTP requests.
	/// </summary>
	public interface IInvokedHttpRequestCollection : IReadOnlyList<IInvokedHttpRequest>
	{
		/// <summary>
		/// Clears the invoked requests collection.
		/// </summary>
		void Clear();
	}
}