using System.Text;
using System.Text.RegularExpressions;

namespace MockHttp.Matchers
{
	internal class RegexPatternMatcher : PatternMatcher
	{
		private readonly Regex _regex;

		public RegexPatternMatcher(string pattern)
		{
			_regex = new Regex(GetMatchPattern(pattern));
		}

		public override bool IsMatch(string value)
		{
			return _regex.IsMatch(value);
		}

		private static string GetMatchPattern(string value)
		{
			var pattern = new StringBuilder();
			bool startsWithWildcard = value[0] == '*';
			if (startsWithWildcard)
			{
				value = value.TrimStart('*');
				pattern.Append(".*");
			}
			else
			{
				pattern.Append('^');
			}

			// ReSharper disable once UseIndexFromEndExpression
			bool endsWithWildcard = value.Length > 0 && value[value.Length - 1] == '*';
			if (endsWithWildcard)
			{
				value = value.TrimEnd('*');
			}

			IEnumerable<string> matchGroups = value
				.Split('*')
				.Where(s => !string.IsNullOrEmpty(s))
				.Select(s => $"({s})");

			pattern.Append(string.Join(".+", matchGroups));

			pattern.Append(endsWithWildcard ? ".*" : "$");

			return pattern.ToString();
		}

	}
}
