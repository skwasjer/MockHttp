using System;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace MockHttp
{
	public class InvokedHttpRequestCollectionTests
	{
		[Fact]
		public void Given_collection_is_not_initialized_when_getting_by_invalid_index_should_throw()
		{
			var sut = new InvokedHttpRequestCollection();

			// Act
			Func<IInvokedHttpRequest> act = () => sut[0];

			// Assert
			act.Should().Throw<IndexOutOfRangeException>();
		}

		[Fact]
		public void Given_collection_is_not_initialized_when_iterating_should_return_nothing()
		{
			var sut = new InvokedHttpRequestCollection();

			// Act & assert
			sut.Should().BeEmpty();
		}

		[Fact]
		public void Given_collection_is_not_initialized_when_getting_count_should_return_0()
		{
			var sut = new InvokedHttpRequestCollection();

			// Act & assert
			sut.Count.Should().Be(0);
		}

		[Fact]
		public void Given_collection_is_cleared_when_getting_by_invalid_index_should_throw()
		{
			var sut = new InvokedHttpRequestCollection
			{
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage())
			};

			// Act
			sut.Clear();
			Func<IInvokedHttpRequest> act = () => sut[0];

			// Assert
			act.Should().Throw<IndexOutOfRangeException>();
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		public void Given_collection_with_items_when_getting_by_valid_index_should_return_item(int index)
		{
			var sut = new InvokedHttpRequestCollection
			{
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage(HttpMethod.Get, "http://uri0")),
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage(HttpMethod.Post, "http://uri1"))
			};

			// Act
			IInvokedHttpRequest item = sut[index];


			// Assert
			item.Request.RequestUri.Should().Be($"http://uri{index}");
		}

		[Fact]
		public void Given_iteration_is_in_process_when_modifying_collection_should_not_throw()
		{
			var sut = new InvokedHttpRequestCollection
			{
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage())
			};

			Action act = () =>
			{
				// Modify collection while iterating.
				foreach (IInvokedHttpRequest _ in sut)
				{
					sut.Add(new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage()));
				}
			};

			// Assert
			act.Should().NotThrow();
		}

		[Fact]
		public void Given_collection_with_items_when_getting_count_should_return_correct_count()
		{
			var sut = new InvokedHttpRequestCollection
			{
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage(HttpMethod.Get, "http://uri0")),
				new InvokedHttpRequest(new HttpCall(), new HttpRequestMessage(HttpMethod.Post, "http://uri1"))
			};

			// Act & assert
			sut.Count.Should().Be(2);
		}
	}
}