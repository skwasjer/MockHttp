using System.Collections.Specialized;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Extensions
{
	public class NameValueCollectionExtensionsTests
	{
		[Fact]
		public void Given_null_when_getting_as_enumerable_should_throw()
		{
			NameValueCollection nameValueCollection = null;

			// ReSharper disable once ExpressionIsAlwaysNull
			Func<int> act = () => nameValueCollection.AsEnumerable().Count();

			// Assert
			act.Should()
				.Throw<ArgumentNullException>()
				.WithParamName(nameof(nameValueCollection));
		}

		[Fact]
		public void Given_nameValueCollection_when_getting_as_enumerable_should_succeed()
		{
			var nameValueCollection = new NameValueCollection
			{
				{ "key1", "value1" },
				{ "key1", "value2" },
				{ "key2", "value3" }
			};
			var expectedKeyValuePairs = new Dictionary<string, IEnumerable<string>>
			{
				{ "key1", new [] { "value1", "value2" } },
				{ "key2", new [] { "value3" } }
			};

			// Act
			var actual = nameValueCollection.AsEnumerable().ToList();

			// Assert
			actual.Should().BeEquivalentTo(expectedKeyValuePairs);
		}
	}
}
