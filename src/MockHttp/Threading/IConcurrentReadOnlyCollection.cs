namespace MockHttp.Threading;

/// <summary>
/// Represents a thread safe read-only collection of elements that can be accessed by index.
/// </summary>
// vNext(breaking: remove obsolete by implementing IReadOnlyList<T> on IInvokedHttpRequestCollection.
[Obsolete("Will be removed in next version.")]
public interface IConcurrentReadOnlyCollection<out T> : IReadOnlyList<T>
{
}
