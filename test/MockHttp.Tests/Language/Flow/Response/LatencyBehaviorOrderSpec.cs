using System.Diagnostics;
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class LatencyBehaviorOrderSpec : ResponseSpec
{
    private Stopwatch _stopwatch;

    protected override void Given(IResponseBuilder with)
    {
        with.ServerTimeout(TimeSpan.FromMilliseconds(100))
            .Latency(NetworkLatency.Between(600, 601));
        _stopwatch = Stopwatch.StartNew();
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(600), "latency overrides server timeout");
        response.Should().HaveStatusCode(HttpStatusCode.RequestTimeout);
        return Task.CompletedTask;
    }
}
