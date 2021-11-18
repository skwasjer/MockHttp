namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request URI.
	/// </summary>
	[Obsolete("Use `RequestUriMatcher`.")]
	public class UrlMatcher : RequestUriMatcher
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UrlMatcher"/> class using specified <paramref name="uri"/>.
		/// </summary>
		/// <param name="uri">The request URI.</param>
		public UrlMatcher(Uri uri) : base(uri)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlMatcher"/> class using specified <paramref name="uriString"/>.
		/// </summary>
		/// <param name="uriString">The request URI or a URI wildcard.</param>
		/// <param name="allowWildcards"><see langword="true"/> to allow wildcards, or <see langword="false"/> if exact matching.</param>
		public UrlMatcher(string uriString, bool allowWildcards = true) : base(uriString, allowWildcards)
		{
		}
	}
}
