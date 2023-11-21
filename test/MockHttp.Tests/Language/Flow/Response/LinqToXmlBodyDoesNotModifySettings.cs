using System.Xml;
using System.Xml.Linq;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public sealed class LinqToXmlBodyDoesNotModifySettings : ResponseSpec
{
    private static readonly XmlWriterSettings OriginalSettings = new() { CloseOutput = true, Async = false };

    protected override void Given(IResponseBuilder with)
    {
        with.XmlBody(XDocument.Parse("<xml/>"), OriginalSettings);
    }

    protected override Task Should(HttpResponseMessage response)
    {
        OriginalSettings.Async.Should().BeFalse();
        OriginalSettings.CloseOutput.Should().BeTrue();
        return Task.CompletedTask;
    }
}
