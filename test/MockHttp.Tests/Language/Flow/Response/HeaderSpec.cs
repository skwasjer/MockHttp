#nullable enable
using System.Globalization;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class HeaderSpec : ResponseSpec
{
    private readonly DateTimeOffset _utcNow;
    private readonly DateTime _now;

    public HeaderSpec()
    {
        _utcNow = DateTimeOffset.UtcNow;
        _now = DateTime.Now;
    }

    protected override void Given(IResponseBuilder with)
    {
        with.Header("Vary", "Accept")
            .Header("Date", _utcNow.AddYears(-1))
            .Header("Content-Length", 123)
            .Header("Content-Language", "nl", "fr")
            .Header("X-Date", _now.AddYears(-1));

        with.Header("Vary", "Content-Encoding")
            .Header("Date", _utcNow)
            .Header("Content-Length", 456)
            .Header("Content-Language", "de")
            .Header("X-Date", _now)
            .Header("X-Null", (object?)null);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        response.Should()
            .HaveHeader("Vary", "Accept,Content-Encoding")
            .And.HaveHeader("Date", _utcNow.ToString("R", DateTimeFormatInfo.InvariantInfo))
            .And.HaveHeader("Content-Length", "456")
            .And.HaveHeader("Content-Language", "nl,fr,de")
            .And.HaveHeader("X-Date", _now.AddYears(-1).ToString("R") + "," + _now.ToString("R"))
            .And.HaveHeader("X-Null", string.Empty);
        return Task.CompletedTask;
    }
}
#nullable restore
