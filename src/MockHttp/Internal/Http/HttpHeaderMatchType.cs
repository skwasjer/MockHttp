namespace MockHttp.Http;

internal enum HttpHeaderMatchType
{
    /// <summary>
    /// Header and value(s) match exactly.
    /// </summary>
    Exact,

    /// <summary>
    /// Header name matches, values are ignored.
    /// </summary>
    HeaderNameOnly,

    /// <summary>
    /// Header name matches and all values of left comparand must be in right.
    /// </summary>
    HeaderNameAndPartialValues
}
