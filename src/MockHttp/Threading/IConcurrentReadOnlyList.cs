using System.Collections.Generic;

namespace MockHttp.Threading
{
	/// <summary>
	/// Represents a thread safe read-only collection of elements that can be accessed by index.
	/// </summary>
	public interface IConcurrentReadOnlyList<out T> : IReadOnlyList<T>
	{
	}
}