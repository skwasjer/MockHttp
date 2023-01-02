using System.Xml;

namespace MockHttp.Language.Flow.Response;

public class LinqToXmlBodyWithIndentationSpec : LinqToXmlBodySpec
{
    public LinqToXmlBodyWithIndentationSpec()
        : base(new XmlWriterSettings
        {
            Indent = true
        })
    {
    }

    protected override string GetExpectedXml()
    {
        return $"<?xml version=\"1.0\" encoding=\"{Encoding.WebName}\"?>" + Environment.NewLine
          + "<doc>" + Environment.NewLine
          + "  <title>Hello</title>" + Environment.NewLine
          + "</doc>";
    }
}
