using System;
using System.Text;

namespace HttpClientMock.HttpRequestMatchers
{
	public class PartialContentMatcher : ContentMatcher
	{
		public PartialContentMatcher(string content)
			: base(content)
		{
			if (content.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		public PartialContentMatcher(string content, Encoding encoding)
			: base(content, encoding)
		{
			if (content.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		public PartialContentMatcher(byte[] content)
			: base(content)
		{
			if (content.Length == 0)
			{
				throw new ArgumentException("Content can not be empty.", nameof(content));
			}
		}

		protected override bool IsMatch(byte[] receivedContent)
		{
			if (receivedContent == null)
			{
				return false;
			}

			return FindIndex(receivedContent, ExpectedContent) != -1;
		}

		// TODO: move to util?
		private static int FindIndex(byte[] data, byte[] pattern, int offset = 0)
		{
			if (pattern.Length > data.Length)
			{
				return -1;
			}

			for (int i = offset; i < data.Length;)
			{
				int j = 0;
				for (; j < pattern.Length; j++)
				{
					if (pattern[j] != data[i])
					{
						break;
					}

					i++;
				}

				if (j == pattern.Length)
				{
					return i - pattern.Length;
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