using System.Diagnostics;
using System.Net;
using MockHttp.IO;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class TransferRateSpec : ResponseSpec
{
    private const int DataSizeInBytes = 256 * 1024; // 256 KB
    private const int BitRate = 512000; // 512 kbps = 64 KB/s
    private static readonly TimeSpan ExpectedTotalTime = TimeSpan.FromSeconds(DataSizeInBytes / ((double)BitRate / 8));

    private readonly byte[] _content = Enumerable.Range(0, DataSizeInBytes)
        .Select((_, index) => (byte)(index % 256))
        .ToArray();

    private readonly Stopwatch _stopwatch = new();

    protected override void Given(IResponseBuilder with)
    {
        with.Body(new RateLimitedStream(new MemoryStream(_content), BitRate));
        _stopwatch.Start();
    }

    protected override async Task Should(HttpResponseMessage response)
    {
        _stopwatch.Stop();
        _stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(ExpectedTotalTime);
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        byte[]? responseContent = await response.Content.ReadAsByteArrayAsync();
        responseContent.Should().BeEquivalentTo(_content, opts => opts.WithStrictOrdering());
    }
}
