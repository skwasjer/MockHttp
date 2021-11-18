using FluentAssertions.Execution;
using FluentAssertions.Specialized;

namespace MockHttp.FluentAssertions;

public static class ExceptionAssertionsExtensions
{
    /// <summary>
    /// Asserts that an <see cref="ArgumentException" /> (or inherited exception) is thrown with the expected parameter name.
    /// </summary>
    /// <typeparam name="TException">The <see cref="ArgumentException" /> (or inherited exception).</typeparam>
    /// <param name="assertion"></param>
    /// <param name="expectedParameterName">The expected parameter name.</param>
    public static ExceptionAssertions<TException> WithParamName<TException>(this ExceptionAssertions<TException> assertion, string expectedParameterName, string because = "", params object[] becauseArgs)
        where TException : ArgumentException
    {
        ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks)
            .ForCondition(assertion.Which.ParamName == expectedParameterName)
            .BecauseOf(because, becauseArgs)
            .FailWith($"Expected {assertion.Which.GetType().Name} with parameter {{0}}{{reason}}, but found {{1}} instead.", expectedParameterName, assertion.Which.ParamName);

        return assertion;
    }

    /// <summary>
    /// Asserts that an <see cref="ArgumentException" /> (or inherited exception) is thrown with the expected parameter name.
    /// </summary>
    /// <typeparam name="TException">The <see cref="ArgumentException" /> (or inherited exception).</typeparam>
    /// <param name="assertion"></param>
    /// <param name="expectedParameterName">The expected parameter name.</param>
    public static async Task<ExceptionAssertions<TException>> WithParamName<TException>(this Task<ExceptionAssertions<TException>> assertion, string expectedParameterName, string because = "", params object[] becauseArgs)
        where TException : ArgumentException
    {
        return (await assertion).WithParamName(expectedParameterName, because, becauseArgs);
    }
}
