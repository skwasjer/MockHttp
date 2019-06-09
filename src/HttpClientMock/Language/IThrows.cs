using System;
using System.ComponentModel;
using HttpClientMock.Language.Flow;

namespace HttpClientMock.Language
{
	/// <summary>
	/// Defines the <c>Throws</c> verb and overloads for throwing exceptions.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IThrows : IFluentInterface
	{
		/// <summary>
		/// Specifies the <paramref name="exception"/> to throw when a request is sent.
		/// </summary>
		/// <param name="exception">The exception to throw.</param>
		IThrowsResult Throws(Exception exception);

		/// <summary>
		/// Specifies the type of exception to throw when a request is sent.
		/// </summary>
		/// <typeparam name="TException">The type of the exception to throw.</typeparam>
		IThrowsResult Throws<TException>()
			where TException : Exception, new();
	}
}
