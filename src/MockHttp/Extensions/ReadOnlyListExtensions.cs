namespace MockHttp
{
	internal static class ReadOnlyListExtensions
	{
		/// <summary>
		/// Checks if <paramref name="source"/> contains <paramref name="pattern"/>.
		/// </summary>
		public static bool Contains<T>(this IReadOnlyList<T> source, IReadOnlyList<T> pattern)
		{
			return Contains(source, pattern, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Checks if <paramref name="source"/> contains <paramref name="pattern"/>.
		/// </summary>
		public static bool Contains<T>(this IReadOnlyList<T> source, IReadOnlyList<T> pattern, IEqualityComparer<T> comparer)
		{
			return IndexOf(source, pattern, 0, comparer) != -1;
		}

		/// <summary>
		/// Gets the first index of the <paramref name="pattern"/> found within <paramref name="source"/>, or returns -1 otherwise.
		/// </summary>
		public static int IndexOf<T>(this IReadOnlyList<T> source, IReadOnlyList<T> pattern, int offset, IEqualityComparer<T> comparer)
		{
			if (pattern.Count > source.Count)
			{
				return -1;
			}

			for (int i = offset; i < source.Count;)
			{
				int j = 0;
				for (; j < pattern.Count; j++)
				{
					if (!comparer.Equals(pattern[j], source[i]))
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
