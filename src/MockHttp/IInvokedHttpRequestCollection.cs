using MockHttp.Threading;

namespace MockHttp
{
	/// <summary>
	/// Represents a collection of invoked HTTP requests.
	/// </summary>
	public interface IInvokedHttpRequestCollection : IConcurrentReadOnlyCollection<IInvokedHttpRequest>
	{
		/// <summary>
		/// Clears the invoked requests collection.
		/// </summary>
		void Clear();
	}
}