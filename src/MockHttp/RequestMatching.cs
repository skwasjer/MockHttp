using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using MockHttp.Matchers;

namespace MockHttp
{
	/// <summary>
	/// A builder to configure request matchers.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class RequestMatching : IFluentInterface
	{
		private readonly List<IAsyncHttpRequestMatcher> _matchers = new List<IAsyncHttpRequestMatcher>();

		internal RequestMatching()
		{
		}

		/// <summary>
		/// Adds a matcher.
		/// </summary>
		/// <param name="matcher">The matcher instance.</param>
		/// <returns>The request matching builder.</returns>
		public RequestMatching With(IAsyncHttpRequestMatcher matcher)
		{
			if (matcher is null)
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
		protected internal virtual void ValidateMatcher(IAsyncHttpRequestMatcher matcher)
		{
			List<IAsyncHttpRequestMatcher> sameTypeMatchers = _matchers
				.Where(m => m.GetType() == matcher.GetType())
				.ToList();

			if (matcher.IsExclusive && sameTypeMatchers.Any() || !matcher.IsExclusive && sameTypeMatchers.Any(m => m.IsExclusive))
			{
				throw new InvalidOperationException($"Cannot add matcher, another matcher of type '{matcher.GetType().FullName}' already is configured.");
			}
		}

		internal IReadOnlyCollection<IAsyncHttpRequestMatcher> Build()
		{
			return new ReadOnlyCollection<IAsyncHttpRequestMatcher>(_matchers.ToArray());
		}
	}
}
