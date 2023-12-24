using System.Diagnostics;
using System.Net;
using MockHttp.IO;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class TransferRateSpec : ResponseSpec
{
    private const int DataSizeInBytes = 256 * 1024; // 256 KB
    protected const int BitRate = 1_024_000; // 1024 kbps = 128 KB/s
    private static readonly TimeSpan ExpectedTotalTime = TimeSpan.FromSeconds(DataSizeInBytes / ((double)BitRate / 8));

    protected readonly byte[] Content = Enumerable.Range(0, DataSizeInBytes)
        .Select((_, index) => (byte)(index % 256))
        .ToArray();

    private readonly Stopwatch _stopwatch = new();

    protected override Task<HttpResponseMessage> When(HttpClient httpClient)
    {
        _stopwatch.Restart();
        return base.When(httpClient);
    }

    protected override void Given(IResponseBuilder with)
    {
        with.Body(new RateLimitedStream(new MemoryStream(Content), BitRate));
    }

    protected sealed override async Task Should(HttpResponseMessage response)
    {
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(ExpectedTotalTime);
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        byte[]? responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(Content, opts => opts.WithStrictOrdering());
    }
}
