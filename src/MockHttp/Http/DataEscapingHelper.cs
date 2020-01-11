using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MockHttp.Http
{
	internal static class DataEscapingHelper
	{
		private const char TokenAmpersand = '&';
		private const char TokenEquals = '=';

		internal static IEnumerable<KeyValuePair<string, IEnumerable<string>>> Parse(string dataEscapedString)
		{
			if (dataEscapedString is null)
			{
				throw new ArgumentNullException(nameof(dataEscapedString));
			}

			if (dataEscapedString.Length == 0)
			{
				return new List<KeyValuePair<string, IEnumerable<string>>>();
			}

			return dataEscapedString
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
						throw new FormatException("The escaped data string format is invalid.");
					}

					string key = Uri.UnescapeDataString(kvp[0]);
					string value = kvp.Length > 1 ? Uri.UnescapeDataString(kvp[1]) : null;
					return new KeyValuePair<string, string>(key, value);
				})
				// Group values for same key.
				.GroupBy(kvp => kvp.Key)
				.Select(g => new KeyValuePair<string, IEnumerable<string>>(g.Key, g.Select(v => v.Value).Where(v => v is { })))
				.ToList();
		}

		internal static string Format(IEnumerable<KeyValuePair<string, string>> items)
		{
			return Format(items.Select(kvp => new KeyValuePair<string, IEnumerable<string>>(kvp.Key, new[] { kvp.Value })));
		}

		internal static string Format(IEnumerable<KeyValuePair<string, IEnumerable<string>>> items)
		{
			var sb = new StringBuilder();
			foreach (KeyValuePair<string, IEnumerable<string>> item in items)
			{
				bool hasValues = item.Value is { } && item.Value.Any();
				if (hasValues)
				{
					foreach (string v in item.Value)
					{
						sb.Append(CreateEscapedKeyValuePair(item.Key, v));
					}

					// Remove last ampersand.
					sb.Remove(sb.Length - 1, 1);
				}
				else
				{
					// No values, so only add key.
					sb.Append(Uri.EscapeDataString(item.Key));
				}

				sb.Append(TokenAmpersand);
			}

			if (sb.Length > 0)
			{
				// Remove last ampersand.
				sb.Remove(sb.Length - 1, 1);
			}

			return sb.ToString();
		}

		private static string CreateEscapedKeyValuePair(string key, string value)
		{
			return Uri.EscapeDataString(key)
			  + TokenEquals
			  + (value is null ? string.Empty : Uri.EscapeDataString(value))
			  + TokenAmpersand;
		}
	}
}
