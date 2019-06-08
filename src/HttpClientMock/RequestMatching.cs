using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HttpClientMock
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class RequestMatching : IFluentInterface
	{
		private readonly List<IHttpRequestMatcher> _matchers = new List<IHttpRequestMatcher>();

		internal RequestMatching()
		{
		}

		public RequestMatching Replace<TMatcher>(IHttpRequestMatcher matcher)
			where TMatcher : IHttpRequestMatcher
		{
			_matchers.RemoveAll(m => m.GetType() == typeof(TMatcher));
			return With(matcher);
		}

		public RequestMatching With(IHttpRequestMatcher matcher)
		{
			_matchers.Add(matcher);
			return this;
		}

		internal IReadOnlyCollection<IHttpRequestMatcher> Build()
		{
			return new ReadOnlyCollection<IHttpRequestMatcher>(_matchers.ToArray());
		}
	}
}