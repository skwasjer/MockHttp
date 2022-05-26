#nullable enable
using System.Diagnostics;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ClientTimeoutSpec : GuardedResponseSpec
{
    private readonly Stopwatch _stopwatch = new();
    private readonly TimeSpan _timeoutAfter = TimeSpan.FromMilliseconds(500);

    protected override void Given(IResponseBuilder with)
    {
        with.ClientTimeout(_timeoutAfter);
        _stopwatch.Start();
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should().ThrowExactlyAsync<TaskCanceledException>();
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(_timeoutAfter);
    }
}
#nullable restore
