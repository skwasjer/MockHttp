using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MockHttp
{
	/// <summary>
	/// A builder to configure request matchers.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class RequestMatching : IFluentInterface
	{
		private readonly List<IHttpRequestMatcher> _matchers = new List<IHttpRequestMatcher>();

		internal RequestMatching()
		{
		}

		/// <summary>
		/// Adds or replaces an existing matcher by type.
		/// </summary>
		/// <param name="matcher">The new matcher instance.</param>
		/// <returns>The request matching builder.</returns>
		public RequestMatching Replace(IHttpRequestMatcher matcher)
		{
			if (matcher == null)
			{
				throw new ArgumentNullException(nameof(matcher));
			}

			_matchers.RemoveAll(m => m.GetType() == matcher.GetType());
			return With(matcher);
		}

		/// <summary>
		/// Adds a matcher.
		/// </summary>
		/// <param name="matcher">The matcher instance.</param>
		/// <returns>The request matching builder.</returns>
		public RequestMatching With(IHttpRequestMatcher matcher)
		{
			if (matcher == null)
			{
				throw new ArgumentNullException(nameof(matcher));
			}

			_matchers.Add(matcher);
			return this;
		}

		internal IReadOnlyCollection<IHttpRequestMatcher> Build()
		{
			return new ReadOnlyCollection<IHttpRequestMatcher>(_matchers.Distinct().ToArray());
		}
	}
}