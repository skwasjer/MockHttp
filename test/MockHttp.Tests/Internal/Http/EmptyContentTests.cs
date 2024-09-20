using System.Net;

namespace MockHttp.Http;

public class EmptyContentTests
{
    private readonly EmptyContent _sut = new();

    [Fact]
    public async Task When_reading_string_it_should_return_empty()
    {
        (await _sut.ReadAsStringAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task When_reading_stream_async_it_should_return_empty()
    {
        // Act
#if NETCOREAPP3_1_OR_GREATER
        await using Stream stream = await _sut.ReadAsStreamAsync();
#else
        using Stream stream = await _sut.ReadAsStreamAsync();
#endif

        // Assert
        stream.Should().NotBeNull();
        stream.Position.Should().Be(0);
        stream.Length.Should().Be(0);
    }

#if NET6_0_OR_GREATER

    [Fact]
    public async Task When_reading_stream_sync_it_should_return_empty()
    {
        // Act
        // ReSharper disable once MethodHasAsyncOverload
        await using Stream stream = _sut.ReadAsStream();

        // Assert
        stream.Should().NotBeNull();
        stream.Position.Should().Be(0);
        stream.Length.Should().Be(0);
    }

    [Fact]
    public async Task Given_that_cancellation_token_is_cancelled_when_reading_stream_it_should_throw()
    {
        // Act
        Func<Task> act = () =>
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            return _sut.ReadAsStreamAsync(cts.Token);
        };

        // Assert
        await act.Should().ThrowExactlyAsync<TaskCanceledException>();
    }

    [Fact]
    public void When_copying_sync_it_should_not_throw()
    {
        Action act = () =>
        {
            using var ms = new MemoryStream();
            // ReSharper disable once MethodHasAsyncOverload
            _sut.CopyTo(ms, Substitute.For<TransportContext>(), CancellationToken.None);
            ms.Length.Should().Be(0);
        };

        act.Should().NotThrow();
    }
#endif

    [Fact]
    public async Task When_copying_async_it_should_not_throw()
    {
        Func<Task> act = async () =>
        {
            using var ms = new MemoryStream();
            await _sut.CopyToAsync(ms);
            ms.Length.Should().Be(0);
        };

        await act.Should().NotThrowAsync();
    }
}
