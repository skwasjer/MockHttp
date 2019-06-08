using System;
using System.ComponentModel;
using HttpClientMock.Language.Flow;

namespace HttpClientMock.Language
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IThrows : IFluentInterface
	{
		IThrowsResult Throws(Exception exception);

		IThrowsResult Throws<TException>()
			where TException : Exception, new();
	}
}
