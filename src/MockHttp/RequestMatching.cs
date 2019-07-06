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

			if (_matchers.Contains(matcher))
			{
				return this;
			}

			ValidateMatcher(matcher);

			_matchers.Add(matcher);
			return this;
		}

		/// <summary>
		/// </summary>
		// ReSharper disable once MemberCanBeProtected.Global
		protected internal virtual void ValidateMatcher(IHttpRequestMatcher matcher)
		{
			List<IHttpRequestMatcher> sameTypeMatchers = _matchers
				.Where(m => m.GetType() == matcher.GetType())
				.ToList();

			if (matcher.IsExclusive && sameTypeMatchers.Any() || !matcher.IsExclusive && sameTypeMatchers.Any(m => m.IsExclusive))
			{
				throw new InvalidOperationException($"Cannot add matcher, another matcher of type '{matcher.GetType().FullName}' already is configured.");
			}
		}

		internal IReadOnlyCollection<IHttpRequestMatcher> Build()
		{
			return new ReadOnlyCollection<IHttpRequestMatcher>(_matchers.ToArray());
		}
	}
}