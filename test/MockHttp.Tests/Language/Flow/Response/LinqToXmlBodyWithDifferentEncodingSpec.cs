using System.Text;
using System.Xml;

namespace MockHttp.Language.Flow.Response;

public class LinqToXmlBodyWithDifferentEncodingSpec : LinqToXmlBodySpec
{
    public LinqToXmlBodyWithDifferentEncodingSpec()
        : base(new XmlWriterSettings
        {
            Encoding = Encoding.ASCII
        })
    {
    }

    protected override string GetSubjectXml()
    {
        return $"<?xml version=\"1.0\" encoding=\"{Encoding.UTF8.WebName}\"?><doc><title>Hello</title></doc>";
    }

    protected override string GetExpectedXml()
    {
        return $"<?xml version=\"1.0\" encoding=\"{Encoding.WebName}\"?><doc><title>Hello</title></doc>";
    }
}
