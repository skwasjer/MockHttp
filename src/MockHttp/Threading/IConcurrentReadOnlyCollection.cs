namespace MockHttp.Threading;

/// <summary>
/// Represents a thread safe read-only collection of elements that can be accessed by index.
/// </summary>
public interface IConcurrentReadOnlyCollection<out T> : IReadOnlyList<T>
{
}
