using FluentAssertions;
using Xunit;

namespace MockHttp.Threading;

public class ConcurrentCollectionTests
{
    [Fact]
    public void Given_collection_is_not_initialized_when_getting_by_invalid_index_should_throw()
    {
        var sut = new ConcurrentCollection<object>();

        // Act
        Func<object> act = () => sut[0];

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Given_collection_is_not_initialized_when_iterating_should_return_nothing()
    {
        var sut = new ConcurrentCollection<object>();

        // Act & assert
        sut.Should().BeEmpty();
    }

    [Fact]
    public void Given_collection_is_not_initialized_when_getting_count_should_return_0()
    {
        var sut = new ConcurrentCollection<object>();

        // Act & assert
        sut.Count.Should().Be(0);
    }

    [Fact]
    public void Given_collection_is_cleared_when_getting_by_invalid_index_should_throw()
    {
        var sut = new ConcurrentCollection<object> { new object() };

        // Act
        sut.Clear();
        Func<object> act = () => sut[0];

        // Assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Given_collection_with_items_when_getting_by_valid_index_should_return_item(int index)
    {
        int expectedValue = index + 1;
        var sut = new ConcurrentCollection<object> { 1, 2 };

        // Act
        object item = sut[index];

        // Assert
        item.Should().Be(expectedValue);
    }

    [Fact]
    public void Given_iteration_is_in_process_when_modifying_collection_should_not_throw()
    {
        var sut = new ConcurrentCollection<object> { new object() };

        Action act = () =>
        {
            // Modify collection while iterating.
            foreach (object _ in sut)
            {
                sut.Add(new object());
            }
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_collection_with_items_when_getting_count_should_return_correct_count()
    {
        var sut = new ConcurrentCollection<object> { new object(), new object() };

        // Act & assert
        sut.Count.Should().Be(2);
    }
}
