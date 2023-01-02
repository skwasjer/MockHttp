using System.Diagnostics;
using System.Net;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class LatencySpec : ResponseSpec
{
    private readonly Stopwatch _stopwatch = new();

    protected override void Given(IResponseBuilder with)
    {
        with.Latency(NetworkLatency.TwoG);
        _stopwatch.Start();
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(300));
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        return Task.CompletedTask;
    }
}
