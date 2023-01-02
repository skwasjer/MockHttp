#nullable enable
using System.Diagnostics;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ClientTimeoutSpec : GuardedResponseSpec
{
    private readonly Stopwatch _stopwatch = new();

    public ClientTimeoutSpec()
        : this(TimeSpan.FromMilliseconds(500))
    {
    }

    protected ClientTimeoutSpec(TimeSpan? timeoutAfter)
    {
        TimeoutAfter = timeoutAfter;
    }

    public TimeSpan? TimeoutAfter { get; set; }

    protected override void Given(IResponseBuilder with)
    {
        with.ClientTimeout(TimeoutAfter);
        _stopwatch.Start();
    }

    protected override async Task ShouldThrow(Func<Task> act)
    {
        await act.Should().ThrowExactlyAsync<TaskCanceledException>();
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeoutAfter ?? TimeSpan.Zero);
    }
}
#nullable restore
