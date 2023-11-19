namespace MockHttp.Http;

internal static class UriExtensions
{
    private const char UriSegmentDelimiter = '/';
    internal static readonly UriKind DotNetRelativeOrAbsolute = Type.GetType("Mono.Runtime") == null ? UriKind.RelativeOrAbsolute : (UriKind)300;

    /// <summary>
    /// If a relative URI, ensures it starts with a forward slash (/). If not, returns the original <paramref name="uri" />.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static Uri EnsureIsRooted(this Uri uri)
    {
        if (uri is null)
        {
            throw new ArgumentNullException(nameof(uri));
        }

        if (uri.IsAbsoluteUri)
        {
            return uri;
        }

        string relUri = uri.ToString();
        if (relUri.Length > 0 && relUri[0] != UriSegmentDelimiter)
        {
            return new Uri($"{UriSegmentDelimiter}{relUri}", UriKind.Relative);
        }

        return uri;
    }
}
