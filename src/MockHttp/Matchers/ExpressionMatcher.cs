using System;
using System.Linq.Expressions;
using System.Net.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request using a custom expression.
	/// </summary>
	public class ExpressionMatcher : IHttpRequestMatcher
	{
		private readonly string _funcDisplay;
		private readonly Func<HttpRequestMessage, bool> _func;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionMatcher"/> class using specified <paramref name="expression"/>.
		/// </summary>
		/// <param name="expression">The expression func to call to check if the request matches.</param>
		public ExpressionMatcher(Expression<Func<HttpRequestMessage, bool>> expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			_func = expression.Compile();
			_funcDisplay = expression.ToString();
		}

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			return _func(request);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Expression: {_funcDisplay}";
		}
	}
}
