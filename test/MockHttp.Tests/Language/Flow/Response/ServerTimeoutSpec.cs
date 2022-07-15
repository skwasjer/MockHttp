#nullable enable
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ServerTimeoutSpec : ResponseSpec
{
    private readonly Stopwatch _stopwatch = new();

    public ServerTimeoutSpec()
        : this(TimeSpan.FromMilliseconds(500))
    {
    }

    protected ServerTimeoutSpec(TimeSpan? timeoutAfter)
    {
        TimeoutAfter = timeoutAfter;
    }

    public TimeSpan? TimeoutAfter { get; }

    protected override void Given(IResponseBuilder with)
    {
        with.ServerTimeout(TimeoutAfter);
        _stopwatch.Start();
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeoutAfter ?? TimeSpan.Zero);
        response.Should().HaveStatusCode(HttpStatusCode.RequestTimeout);
        return Task.CompletedTask;
    }
}
#nullable restore
