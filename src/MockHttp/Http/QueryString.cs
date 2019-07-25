using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MockHttp.Http
{
	[DebuggerDisplay("{ToString(),nq}")]
	internal class QueryString : Dictionary<string, IEnumerable<string>>
	{
		private static readonly Uri UnknownBaseUri = new Uri("http://0.0.0.0");

		private const char TokenQuestionMark = '?';
		private const char TokenAmpersand = '&';
		private const char TokenEquals = '=';
		private const char TokenTerminator = '#';

		public QueryString()
		{
		}

		public QueryString(IEnumerable<KeyValuePair<string, IEnumerable<string>>> values)
			: this()
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			foreach (KeyValuePair<string, IEnumerable<string>> kvp in values)
			{
				if (string.IsNullOrEmpty(kvp.Key))
				{
					throw new FormatException("Key can not be null or empty.");
				}

				// TODO: implement/override Add, because empty string for key is possible but not allowed. Even though we checked here, still possible to call Add externally.
				// Accept null values enumerable, but then use empty list.
				// Null values in values enumerable are filtered.
				Add(kvp.Key, kvp.Value?.Where(v => v != null).ToList() ?? new List<string>());
			}
		}

		public override string ToString()
		{
			return FormatQueryString(this);
		}

		public static QueryString Parse(string queryString)
		{
			if (queryString == null)
			{
				throw new ArgumentNullException(nameof(queryString));
			}

			if (!TryGetQueryFromUri(queryString, out string qs))
			{
				qs = queryString;

				// If query string contains terminator, strip it off.
				qs = qs.Split(TokenTerminator)[0];
			}

			// If begins with question mark, strip it off.
			qs = qs.TrimStart(TokenQuestionMark);

			if (qs.Length == 0)
			{
				return new QueryString();
			}

			return new QueryString(
				qs
					.Split(TokenAmpersand)
					.Select(segments =>
					{
						string[] kvp = segments.Split(TokenEquals);
						if (kvp[0].Length == 0)
						{
							throw new FormatException("Key can not be null or empty.");
						}

						if (kvp.Length > 2)
						{
							throw new FormatException("The query string format is invalid.");
						}

						string key = Uri.UnescapeDataString(kvp[0]);
						string value = kvp.Length > 1 ? Uri.UnescapeDataString(kvp[1]) : null;
						return new KeyValuePair<string, string>(key, value);
					})
					// Group values for same key.
					.GroupBy(kvp => kvp.Key)
					.Select(g => new KeyValuePair<string, IEnumerable<string>>(g.Key, g.Select(v => v.Value)))
				);
		}

		private static bool TryGetQueryFromUri(string uri, out string queryString)
		{
#if NETSTANDARD1_1
			if (uri.Contains(TokenQuestionMark.ToString())
#else
			if (uri.Contains(TokenQuestionMark.ToString(CultureInfo.InvariantCulture))
#endif
				&& Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri u))
			{
				if (!u.IsAbsoluteUri)
				{
					u = new Uri(UnknownBaseUri, uri);
				}

				queryString = u.Query;
				return true;
			}

			queryString = null;
			return false;
		}

		private static string FormatQueryString(IDictionary<string, IEnumerable<string>> queryString)
		{
			if (queryString.Count == 0)
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			sb.Append(TokenQuestionMark);
			foreach (KeyValuePair<string, IEnumerable<string>> qsPair in queryString)
			{
				bool hasValues = qsPair.Value != null && qsPair.Value.Any();
				if (hasValues)
				{
					foreach (string v in qsPair.Value)
					{
						sb.Append(CreateQueryStringKeyValuePair(qsPair.Key, v));
					}

					// Remove last ampersand.
					sb.Remove(sb.Length - 1, 1);
				}
				else
				{
					// No values, so only add key.
					sb.Append(Uri.EscapeDataString(qsPair.Key));
				}

				sb.Append(TokenAmpersand);
			}

			// Remove last ampersand.
			sb.Remove(sb.Length - 1, 1);

			return sb.ToString();
		}

		private static string CreateQueryStringKeyValuePair(string key, string value)
		{
			return Uri.EscapeDataString(key) 
			  + TokenEquals 
			  + Uri.EscapeDataString(value) 
			  + TokenAmpersand;
		}
	}
}
