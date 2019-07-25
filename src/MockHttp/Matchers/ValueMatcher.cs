using System.Net.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Base class that matches a request by a value.
	/// </summary>
	/// <typeparam name="T">The type of the value to match.</typeparam>
	public abstract class ValueMatcher<T> : HttpRequestMatcher
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValueMatcher{T}"/> class using specified <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to match a request by.</param>
		protected ValueMatcher(T value)
		{
			Value = value;
		}

		/// <summary>
		/// Gets the expected value.
		/// </summary>
		protected T Value { get; }
	}
}
