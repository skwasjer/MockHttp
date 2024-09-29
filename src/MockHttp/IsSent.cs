using System.Globalization;

namespace MockHttp;

/// <summary>
/// Defines the number of times a request is allowed to be sent.
/// </summary>
public sealed class IsSent
{
    private readonly Func<int, bool> _evaluator;
    private readonly string _name;
    private readonly string _message;

    private IsSent(Func<int, bool> evaluator, string name, string message)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _name = name;
        _message = message;
    }

    /// <summary>
    /// Specifies that the mocked request should be sent <paramref name="callCount" /> time at minimum.
    /// </summary>
    public static IsSent AtLeast(int callCount)
    {
        if (callCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(callCount));
        }

        return new IsSent(
            i => i >= callCount,
            $"{nameof(AtLeast)}({callCount})",
            $"Expected request to have been sent at least {callCount} time(s){{0}}, but was sent only {{1}} time(s)."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should be sent one time at minimum.
    /// </summary>
    public static IsSent AtLeastOnce()
    {
        return new IsSent(
            i => i >= 1,
            nameof(AtLeastOnce),
            "Expected request to have been sent at least once{0}, but it was not."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should be sent <paramref name="callCount" /> time at maximum.
    /// </summary>
    public static IsSent AtMost(int callCount)
    {
        if (callCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(callCount));
        }

        return new IsSent(
            i => i <= callCount,
            $"{nameof(AtMost)}({callCount})",
            $"Expected request to have been sent at most {callCount} time(s){{0}}, but was actually sent {{1}} time(s)."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should be sent one time at maximum.
    /// </summary>
    public static IsSent AtMostOnce()
    {
        return new IsSent(
            i => i <= 1,
            nameof(AtMostOnce),
            "Expected request to have been sent at most once{0}, but was actually sent {1} time(s)."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should be sent exactly <paramref name="callCount" /> times.
    /// </summary>
    public static IsSent Exactly(int callCount)
    {
        if (callCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(callCount));
        }

        return new IsSent(
            i => i == callCount,
            $"{nameof(Exactly)}({callCount})",
            $"Expected request to have been sent exactly {callCount} time(s){{0}}, but was actually sent {{1}} time(s)."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should not be sent.
    /// </summary>
    public static IsSent Never()
    {
        return new IsSent(
            i => i == 0,
            nameof(Never),
            "Expected request to have never been sent{0}, but was actually sent {1} time(s)."
        );
    }

    /// <summary>
    /// Specifies that the mocked request should be sent exactly one time.
    /// </summary>
    public static IsSent Once()
    {
        return new IsSent(
            i => i == 1,
            nameof(Once),
            "Expected request to have been sent exactly once{0}, but was actually sent {1} time(s)."
        );
    }

    internal bool Verify(int callCount)
    {
        return _evaluator(callCount);
    }

    internal string GetErrorMessage(int actualCount, string becauseMessage)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            _message,
            becauseMessage,
            actualCount
        );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetType().Name}.{_name}";
    }
}
