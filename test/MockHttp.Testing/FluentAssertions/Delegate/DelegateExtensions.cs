using FluentAssertions;

namespace MockHttp.FluentAssertions.Delegate;

public static class DelegateExtensions
{
	public static DelegateAssertions Should(this System.Delegate instance)
	{
		return new DelegateAssertions(instance, new AggregateExceptionExtractor());
	}
}