#nullable enable
using System.Diagnostics;
using System.Net;
using FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class ServerTimeoutSpec : ResponseSpec
{
    private readonly Stopwatch _stopwatch = new();
    private readonly TimeSpan _timeoutAfter = TimeSpan.FromMilliseconds(500);

    protected override void Given(IResponseBuilder with)
    {
        with.ServerTimeout(_timeoutAfter);
        _stopwatch.Start();
    }

    protected override Task Should(HttpResponseMessage response)
    {
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(_timeoutAfter);
        response.Should().HaveStatusCode(HttpStatusCode.RequestTimeout);
        return Task.CompletedTask;
    }
}
#nullable restore
