using System.ComponentModel;

namespace MockHttp.Language.Flow;

[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class HttpRequestSetupPhrase : SetupPhrase<ISequenceResponseResult, ISequenceThrowsResult>, IConfiguredRequest, IFluentInterface
{
	public HttpRequestSetupPhrase(HttpCall setup)
		: base(setup)
	{
	}

	public void Verifiable()
	{
		Setup.SetVerifiable(null);
	}

	public void Verifiable(string because)
	{
		if (because is null)
		{
			throw new ArgumentNullException(nameof(because));
		}

		Setup.SetVerifiable(because);
	}

	public ICallbackResult<ISequenceResponseResult, ISequenceThrowsResult> Callback(Action<HttpRequestMessage> callback)
	{
		Setup.SetCallback(callback);
		return this;
	}

	public ICallbackResult<ISequenceResponseResult, ISequenceThrowsResult> Callback(Action callback)
	{
		return Callback(_ => callback());
	}
}