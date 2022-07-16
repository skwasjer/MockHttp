namespace MockHttp.Http;

/// <summary>
/// Common media types.
/// </summary>
public static class MediaTypes
{
    internal const string DefaultMediaType = PlainText;
#pragma warning disable CS1591
    public const string FormUrlEncoded = "application/x-www-form-urlencoded";
    public const string Html = "text/html";
    public const string Json = "application/json";
    public const string JsonProblemDetails = "application/problem+json";
    public const string MultipartFormData = "multipart/form-data";
    public const string OctetStream = "application/octet-stream";
    public const string PlainText = "text/plain";
    public const string Xml = "application/xml";
    public const string XmlProblemDetails = "application/problem+xml";
#pragma warning restore CS1591
}
