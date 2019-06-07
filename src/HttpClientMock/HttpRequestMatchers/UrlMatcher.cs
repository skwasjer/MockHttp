using System;
using System.Diagnostics;
using System.Net.Http;
using DotNet.Globbing;

namespace HttpClientMock.HttpRequestMatchers
{
	public class UrlMatcher : IHttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _requestUri;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Glob _glob;

		public UrlMatcher(string requestUri)
		{
			_requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));

			// Parse with default options, in case externally default options were set.
			var options = new GlobOptions();
			_glob = Glob.Parse(requestUri, options);
		}

		public bool IsMatch(HttpRequestMessage request)
		{
			return _glob.IsMatch(request.RequestUri.ToString());
		}

		public override string ToString()
		{
			return $"Uri: {_requestUri}";
		}
	}
}
