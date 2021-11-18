using System.Linq.Expressions;
using MockHttp.Responses;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request using a custom expression.
	/// </summary>
	public class ExpressionMatcher : HttpRequestMatcher
	{
		private readonly string _funcDisplay;
		private readonly Func<HttpRequestMessage, bool> _func;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionMatcher"/> class using specified <paramref name="expression"/>.
		/// </summary>
		/// <param name="expression">The expression func to call to check if the request matches.</param>
		public ExpressionMatcher(Expression<Func<HttpRequestMessage, bool>> expression)
		{
			if (expression is null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			_func = expression.Compile();
			_funcDisplay = expression.ToString();
		}

		/// <inheritdoc />
		public override bool IsMatch(MockHttpRequestContext requestContext)
		{
			if (requestContext is null)
			{
				throw new ArgumentNullException(nameof(requestContext));
			}

			return _func(requestContext.Request);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Expression: {_funcDisplay}";
		}
	}
}
