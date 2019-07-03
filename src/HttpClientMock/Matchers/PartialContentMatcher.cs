using System;
using System.Collections.Generic;
using System.Text;

namespace HttpClientMock.Matchers
{
	public class PartialContentMatcher : ContentMatcher
	{
		public PartialContentMatcher(string content)
			: base(content)
		{
			if (Value.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		public PartialContentMatcher(string content, Encoding encoding)
			: base(content, encoding)
		{
			if (Value.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		public PartialContentMatcher(byte[] content)
			: base(content)
		{
			if (Value.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		protected override bool IsMatch(byte[] receivedContent)
		{
			return Contains(receivedContent, Value);
		}

		// TODO: move to util/extension?
		private static bool Contains<T>(IReadOnlyList<T> source, IReadOnlyList<T> value)
		{
			return Contains(source, value, EqualityComparer<T>.Default);
		}

		private static bool Contains<T>(IReadOnlyList<T> source, IReadOnlyList<T> value, IEqualityComparer<T> comparer)
		{
			return IndexOf(source, value, 0, comparer) != -1;
		}

		private static int IndexOf<T>(IReadOnlyList<T> data, IReadOnlyList<T> pattern, int offset, IEqualityComparer<T> comparer)
		{
			if (pattern.Count > data.Count)
			{
				return -1;
			}

			for (int i = offset; i < data.Count;)
			{
				int j = 0;
				for (; j < pattern.Count; j++)
				{
					if (!comparer.Equals(pattern[j], data[i]))
					{
						break;
					}

					i++;
				}

				if (j == pattern.Count)
				{
					return i - pattern.Count;
				}

				if (j != 0)
				{
					continue;
				}

				i++;
			}

			return -1;
		}
	}
}