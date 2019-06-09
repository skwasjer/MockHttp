﻿using System.ComponentModel;

namespace HttpClientMock.Language.Flow
{
	/// <summary>
	/// Implements the fluent API.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IConfiguredRequest : IResponds, IResponseResult, IThrows, IThrowsResult, IRespondsThrows, ICallback, ICallbackResult, IFluentInterface
	{
	}
}