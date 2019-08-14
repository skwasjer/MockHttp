using MockHttp.Threading;

namespace MockHttp
{
	/// <summary>
	/// Represents a collection of invoked HTTP requests.
	/// </summary>
	public interface IInvokedHttpRequestCollection : IConcurrentReadOnlyList<IInvokedHttpRequest>
	{
		/// <summary>
		/// Clears the invoked requests collection.
		/// </summary>
		void Clear();
	}
}