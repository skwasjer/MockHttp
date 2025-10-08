using System.Globalization;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HeaderSpec : ResponseSpec
{
    private readonly DateTimeOffset _utcNow = DateTimeOffset.UtcNow;
    private readonly DateTime _now = DateTime.Now;

    protected override void Given(IResponseBuilder with)
    {
        with.Header("Vary", "Accept")
            .Header("Date", _utcNow.AddYears(-1))
            .Header("Content-Length", 123)
            .Header("Content-Language", "nl", "fr")
            .Header(new KeyValuePair<string, DateTime>("X-Date", _now.AddYears(-1)));

        with.Header("Vary", "Content-Encoding")
            .Header("Date", _utcNow)
            .Header("Content-Length", 456)
            .Header(new KeyValuePair<string, IEnumerable<string>>("Content-Language", ["de", "es"]))
            .Header("X-Date", _now)
            .Header("X-Empty", string.Empty)
            .Header("X-Null", (object?)null);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should()
            .HaveHeader("Vary", "Accept,Content-Encoding")
            .And.HaveHeader("Date", _utcNow.ToString("R", DateTimeFormatInfo.InvariantInfo))
            .And.HaveHeader("Content-Length", "456")
            .And.HaveHeader("Content-Language", "nl,fr,de,es")
            .And.HaveHeader("X-Date", [_now.AddYears(-1).ToString("R"), _now.ToString("R")])
            .And.HaveHeader("X-Empty", string.Empty)
            .And.HaveHeader("X-Null", string.Empty);
        response.Headers.Count().Should().Be(5);
        response.Content.Headers.Count().Should().Be(2);
        return Task.CompletedTask;
    }
}
