#nullable enable
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using MockHttp.FluentAssertions;
using MockHttp.Http;
using MockHttp.Specs;

namespace MockHttp.Language.Flow.Response;

public class LinqToXmlBodySpec : ResponseSpec
{
    private readonly XmlWriterSettings? _settings;

    public LinqToXmlBodySpec()
        : this(null)
    {
    }

    protected Encoding Encoding { get; }

    internal LinqToXmlBodySpec(XmlWriterSettings? settings = null)
    {
        _settings = settings;
        Encoding = settings?.Encoding ?? Encoding.UTF8;
    }

    protected sealed override void Given(IResponseBuilder with)
    {
        with.XmlBody(XDocument.Parse(GetSubjectXml()), _settings);
    }

    protected virtual string GetSubjectXml()
    {
        return $"<?xml version=\"1.0\" encoding=\"{Encoding.WebName}\"?><doc><title>Hello</title></doc>";
    }

    protected virtual string GetExpectedXml()
    {
        return GetSubjectXml();
    }

    protected sealed override Task Should(HttpResponseMessage response)
    {
        return response
            .Should()
            .HaveContentType(MediaTypes.Xml, Encoding)
            .And.HaveContentAsync(Encoding.GetString(Encoding.GetPreamble()) + GetExpectedXml(), Encoding);
    }
}
#nullable restore
