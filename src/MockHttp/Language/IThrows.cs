using System.ComponentModel;
using MockHttp.Language.Flow;

namespace MockHttp.Language
{
	/// <summary>
	/// Defines the <c>Throws</c> verb and overloads for throwing exceptions.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IThrows<out TResult> : IFluentInterface
		where TResult : IThrowsResult
	{
		/// <summary>
		/// Specifies the <paramref name="exception"/> to throw when a request is sent.
		/// </summary>
		/// <param name="exception">The exception to throw.</param>
		TResult Throws(Exception exception);

		/// <summary>
		/// Specifies the type of exception to throw when a request is sent.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to throw.</typeparam>
		TResult Throws<TException>()
			where TException : Exception, new();
	}
}
