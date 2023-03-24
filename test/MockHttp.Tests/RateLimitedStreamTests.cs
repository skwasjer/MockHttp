using System.Diagnostics;
using MockHttp.IO;
using Moq.Protected;
using Xunit.Abstractions;

namespace MockHttp;

public sealed class RateLimitedStreamTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const int DataSizeInBytes = 512 * 1024; // 512 KB
    private const int BitRate = 1024000; // 1024 kbps = 128 KB/s
    private static readonly TimeSpan ExpectedTotalTime = TimeSpan.FromSeconds(DataSizeInBytes / ((double)BitRate / 8)); // ~ 4sec

    private readonly MemoryStream _actualStream;
    private readonly RateLimitedStream _sut;

    private readonly byte[] _content = Enumerable.Range(0, DataSizeInBytes)
        .Select((_, index) => (byte)(index % 256))
        .ToArray();

    public RateLimitedStreamTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _actualStream = new MemoryStream(_content);
        _sut = new RateLimitedStream(_actualStream, BitRate);
    }

    [Fact]
    public void When_reading_from_stream_it_should_be_rate_limited()
    {
        using var ms = new MemoryStream();

        byte[] buffer = new byte[4096];
        var sw = Stopwatch.StartNew();
        long totalBytesRead = 0;
        int bytesRead;
        int readCount = 0;
        while ((bytesRead = _sut.Read(buffer, 0, buffer.Length)) > 0)
        {
            totalBytesRead += bytesRead;
            readCount++;
#if DEBUG
            _testOutputHelper.WriteLine("Read: {0:000}, Time: {1}, Total bytes read: {2}/{3}", readCount, sw.Elapsed, totalBytesRead, DataSizeInBytes);
#endif
            ms.Write(buffer, 0, bytesRead);
        }
        sw.Stop();

        sw.Elapsed.Should().BeCloseTo(ExpectedTotalTime, TimeSpan.FromMilliseconds(ExpectedTotalTime.TotalMilliseconds * 0.05), "it can be within 5% of the expected total time to read the rate limited stream");
        ms.ToArray().Should().BeEquivalentTo(_content, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void Given_that_next_block_read_is_0_when_reading_last_block_it_should_exit()
    {
        byte[] sourceBuffer = { 0, 1, 2, 3 };
        using var actualStream = new MemoryStream(sourceBuffer);
        using var sut = new RateLimitedStream(actualStream, 128);

        byte[] buffer = new byte[4096];
        sut.Read(buffer, 0, buffer.Length).Should().Be(sourceBuffer.Length);
        buffer.Should().StartWith(sourceBuffer);
        sut.Read(buffer, 0, buffer.Length).Should().Be(0);
    }

    [Theory]
    [InlineData(100, 100)]
    [InlineData(1000, 1000 - 768)]
    public void Given_that_position_is_set_when_reading_next_byte_it_should_return_expected_and_advance(long newPosition, byte expectedByte)
    {
        // Act
        _sut.Position = newPosition;
        int nextByte = _sut.ReadByte();

        // Assert
        nextByte.Should().Be(expectedByte);
        _sut.Position.Should().Be(newPosition + 1);
    }

    [Fact]
    public void Given_that_actual_stream_is_null_when_creating_instance_it_should_throw()
    {
        Stream? actualStream = null;

        // Act
        Func<RateLimitedStream> act = () => new RateLimitedStream(actualStream!, 1024);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName(nameof(actualStream));
    }

    [Fact]
    public void Given_that_actual_stream_is_not_readable_when_creating_instance_it_should_throw()
    {
        var actualStream = new Mock<Stream>();
        actualStream
            .Setup(m => m.CanRead)
            .Returns(false)
            .Verifiable();

        // Act
        Func<RateLimitedStream> act = () => new RateLimitedStream(actualStream.Object, 1024);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("Cannot read from stream.*")
            .WithParameterName(nameof(actualStream));
        actualStream.Verify();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(127)]
    public void Given_that_bitRate_is_invalid_when_creating_instance_it_should_throw(int bitRate)
    {
        // Act
        Func<RateLimitedStream> act = () => new RateLimitedStream(Stream.Null, bitRate);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage($"Bit rate must be higher than or equal to {RateLimitedStream.MinBitRate}.*")
            .WithParameterName(nameof(bitRate));
    }

    [Fact]
    public void It_should_set_have_expected_properties()
    {
        _actualStream.Should().BeOfType<MemoryStream>();
        _sut.CanRead.Should().BeTrue();
        _sut.CanWrite.Should().BeFalse("even though actual stream supports writing, we do not");
        _sut.CanSeek.Should().BeTrue();

        _actualStream.Length.Should().NotBe(0);
        _sut.Length.Should().Be(_actualStream.Length);

        _actualStream.Position = 1000;
        _sut.Position.Should().Be(1000);
        _sut.Position = 100;
        _actualStream.Position.Should().Be(100);

        _sut.Seek(2000, SeekOrigin.Current);
        _actualStream.Position.Should().Be(2100);
    }

    [Fact]
    public void When_writing_it_should_throw()
    {
        // Act
        Action act = () => _sut.Write(_content, 0, 1024);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void When_setting_length_it_should_throw()
    {
        // Act
        Action act = () => _sut.SetLength(1024);

        // Assert
        act.Should().ThrowExactly<NotSupportedException>();
    }

    [Fact]
    public void When_flushing_it_should_flush_underlying()
    {
        var streamMock = new Mock<MemoryStream> { CallBase = true };
        using MemoryStream? stream = streamMock.Object;
        var sut = new RateLimitedStream(stream, 1024);

        // Act
        sut.Flush();

        // Assert
        streamMock.Verify(m => m.Flush(), Times.Once);
    }

    [Fact]
    public void When_disposing_it_should_dispose_underlying()
    {
        var streamMock = new Mock<MemoryStream> { CallBase = true };
        using MemoryStream? stream = streamMock.Object;
        var sut = new RateLimitedStream(stream, 1024);

        // Act
        sut.Dispose();

        // Assert
        streamMock
            .Protected()
            .Verify("Dispose", Times.Once(), true, true);
    }

    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _actualStream?.Dispose();
        _sut?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }
}
