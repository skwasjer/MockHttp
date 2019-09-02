using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// A builder to configure request matchers, accepting all matchers.
	/// </summary>
	internal class AnyRequestMatching : RequestMatching
	{
		protected internal override void ValidateMatcher(IAsyncHttpRequestMatcher matcher)
		{
			// Ignore validation.
		}
	}
}